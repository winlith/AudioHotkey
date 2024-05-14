using Windows.Win32;
using Windows.Win32.System.Com;
using Windows.Win32.Media.Audio;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.UI.Shell.PropertiesSystem;
using Windows.Win32.System.Variant;
using Windows.Win32.Foundation;

namespace AudioHotkey.Common
{
    public static class AudioManager
    {
        private static readonly Dictionary<AudioDeviceType, EDataFlow> flows = new()
        {
            { AudioDeviceType.Playback, EDataFlow.eRender },
            { AudioDeviceType.Listen, EDataFlow.eCapture }
        };

        private static IMMDeviceEnumerator? GetDeviceEnumerator()
        {
            Guid CLSID_MMDeviceEnumerator = typeof(MMDeviceEnumerator).GUID;
            Guid IID_IMMDeviceEnumerator = typeof(IMMDeviceEnumerator).GUID;

            var hr = PInvoke.CoCreateInstance(CLSID_MMDeviceEnumerator, null, CLSCTX.CLSCTX_INPROC_SERVER, IID_IMMDeviceEnumerator, out var pEnumerator);
            if (hr < 0)
            {
                return null;
            }

            return pEnumerator as IMMDeviceEnumerator;
        }

        public static bool SetListening(string deviceId, bool enabled)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                try
                {
                    Guid listeningGuid = Guid.Parse("24DBB0FC-9311-4B3D-9CF0-18FF155639D4");
                    uint listeningPid = 1;

                    var enumerator = GetDeviceEnumerator();
                    enumerator.GetDevice(deviceId, out var pDevice);
                    pDevice.OpenPropertyStore(STGM.STGM_READWRITE, out var pPropertyStore);
                    PROPERTYKEY propKey = new()
                    {
                        fmtid = listeningGuid,
                        pid = listeningPid
                    };
                    PROPVARIANT propVar = new();
                    propVar.Anonymous.Anonymous.vt = VARENUM.VT_BOOL;
                    propVar.Anonymous.Anonymous.Anonymous.boolVal = enabled;
                    pPropertyStore.SetValue(propKey, propVar);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }

        private static IList<AudioDevice>? GetAllDevices(AudioDeviceType type)
        {
            Guid friendlyNameGuid = Guid.Parse("a45c254e-df1c-4efd-8020-67d146a850e0");
            uint friendlyNamePid = 14;
            List<AudioDevice> devices = new();

            var enumerator = GetDeviceEnumerator();
            if (enumerator == null) return null;
            enumerator.EnumAudioEndpoints(flows.GetValueOrDefault(type), DEVICE_STATE.DEVICE_STATE_ACTIVE | DEVICE_STATE.DEVICE_STATE_UNPLUGGED, out var allDevices);
            if(allDevices == null) return null;
            allDevices.GetCount(out var deviceCount);
            for(uint i = 0; i < deviceCount; i++)
            {
                allDevices.Item(i, out var device);
                device.GetId(out var id);
                device.OpenPropertyStore(STGM.STGM_READ, out var propStore);
                propStore.GetValue(new PROPERTYKEY() { fmtid = friendlyNameGuid, pid = friendlyNamePid}, out var propVar);
                var friendlyName = propVar.Anonymous.Anonymous.Anonymous.pwszVal.ToString();
                devices.Add(new AudioDevice { Id = id.ToString(), Name = friendlyName });
            }
            return devices;
        }

        public static IList<AudioDevice>? GetAllPlaybackDevices()
        {
            return GetAllDevices(AudioDeviceType.Playback);
        }

        public static IList<AudioDevice>? GetAllListenDevices()
        {
            return GetAllDevices(AudioDeviceType.Listen);
        }

        public static string? GetCurrentPlaybackDeviceId()
        {
            var enumerator = GetDeviceEnumerator();
            if(enumerator == null) return null;
            enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out var defaultDevice);
            defaultDevice.GetId(out var id);
            return id.ToString();
        }

        public static void SetPlaybackDevice(string deviceId)
        {
            var policyConfigClient = new PolicyConfigClient();
            policyConfigClient.SetDefaultEndpoint(deviceId, ERole.eConsole);
            policyConfigClient.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
            policyConfigClient.SetDefaultEndpoint(deviceId, ERole.eCommunications);
        }
    }
}
