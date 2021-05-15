#pragma once
#include "stdafx.h"
#include <thread>
#include "Shared/SettingTypes.h"
#include "Utilities/AutoResetEvent.h"
#include "Utilities/SimpleLock.h"

class IRenderingDevice;
class Emulator;

class IVideoRecorder;
enum class VideoCodec;

class VideoRenderer
{
private:
	shared_ptr<Emulator> _emu;

	AutoResetEvent _waitForRender;
	unique_ptr<std::thread> _renderThread;
	IRenderingDevice* _renderer = nullptr;
	atomic<bool> _stopFlag;
	SimpleLock _stopStartLock;

	uint32_t _rendererWidth = 512;
	uint32_t _rendererHeight = 480;

	shared_ptr<IVideoRecorder> _recorder;

	void RenderThread();

public:
	VideoRenderer(shared_ptr<Emulator> emu);
	~VideoRenderer();

	FrameInfo GetRendererSize();
	void SetRendererSize(uint32_t width, uint32_t height);

	void StartThread();
	void StopThread();

	void UpdateFrame(void *frameBuffer, uint32_t width, uint32_t height);
	void RegisterRenderingDevice(IRenderingDevice *renderer);
	void UnregisterRenderingDevice(IRenderingDevice *renderer);

	void StartRecording(string filename, VideoCodec codec, uint32_t compressionLevel);
	void AddRecordingSound(int16_t* soundBuffer, uint32_t sampleCount, uint32_t sampleRate);
	void StopRecording();
	bool IsRecording();
};