﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using System;

namespace Mesen.Debugger.Controls
{
	public class MesenTextEditor : TextEditor, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TextEditor);

		public static readonly StyledProperty<string> TextBindingProperty = AvaloniaProperty.Register<MesenTextEditor, string>(nameof(TextBinding), "", defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

		public string TextBinding
		{
			get { return GetValue(TextBindingProperty); }
			set { SetValue(TextBindingProperty, value); }
		}

		public ScrollViewer? ScrollViewer { get; private set; }

		public double VerticalScrollBarValue
		{
			get => ScrollViewer!.Offset.Y;
			set => ScrollViewer!.Offset = ScrollViewer.Offset.WithY(value);
		}

		public event EventHandler<ScrollChangedEventArgs>? ScrollChanged
		{
			add => ScrollViewer!.ScrollChanged += value;
			remove => ScrollViewer!.ScrollChanged -= value;
		}

		static MesenTextEditor()
		{
			TextBindingProperty.Changed.AddClassHandler<MesenTextEditor>((x, e) => {
				if(x.Text != (string?)e.NewValue) {
					x.Text = (string)e.NewValue!;
				}
			});
		}
		public MesenTextEditor()
		{
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if(TextBinding != Text) {
				TextBinding = Text;
			}
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);
			ScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
		}

		public void ScrollLineToTop(int line)
		{
			VerticalScrollBarValue = line * TextArea.TextView.DefaultLineHeight;
		}

		/*public MesenTextEditor()
		{
			TextArea.TextEntered += TextArea_TextEntered;
			TextArea.TextEntering += TextArea_TextEntering;
		}
				
		private CompletionWindow? _completionWindow;
		private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
		{
			if(e.Text?.Length > 0 && _completionWindow != null) {
				if(!char.IsLetterOrDigit(e.Text[0])) {
					// Whenever a non-letter is typed while the completion window is open,
					// insert the currently selected element.
					_completionWindow.CompletionList.RequestInsertion(e);
				}
			}
			// Do not set e.Handled=true.
			// We still want to insert the character that was typed.
		}

		private void TextArea_TextEntered(object? sender, TextInputEventArgs e)
		{
			if(e.Text == ".") {
				// Open code completion after the user has pressed dot:
				_completionWindow = new CompletionWindow(TextArea);
				IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
				data.Add(new MyCompletionData("Item1"));
				data.Add(new MyCompletionData("Item2"));
				data.Add(new MyCompletionData("Item3"));
				_completionWindow.Show();

				var loc = TextArea.TextView.GetVisualPosition(TextArea.Caret.Position, VisualYPosition.LineBottom);
				_completionWindow.Host?.ConfigurePosition(this, PlacementMode.Top, new Point(0,0));
				
				_completionWindow.Closed += delegate {
					_completionWindow = null;
				};
			}
		}

		public class MyCompletionData : ICompletionData
		{
			public MyCompletionData(string text)
			{
				this.Text = text;
			}

			public IBitmap Image
			{
				get { return null; }
			}

			public string Text { get; private set; }

			// Use this property if you want to show a fancy UIElement in the list.
			public object Content
			{
				get { return new TextBlock() { Text = this.Text }; }
			}

			public object Description
			{
				get { return "Description for " + this.Text; }
			}

			public double Priority => 1.0;

			public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
			{
				textArea.Document.Replace(completionSegment, this.Text);
			}
		}*/
	}
}