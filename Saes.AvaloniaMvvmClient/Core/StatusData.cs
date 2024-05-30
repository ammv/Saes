using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Core
{
    public enum StatusDataKind
    {
        Nothing,
        LoadingUI,
        LoadingData,
        SendingGrpcRequest,
        HandlingGrpcResponse,
        Error,
        Ok
    }
    public class StatusData
    {
        private sealed class IconData
        {
            public IconData(string key)
            {
                Key = key;
            }

            public string Key { get; set; }
            public StreamGeometry Icon { get; set; }
            
        }
        private static Dictionary<StatusDataKind, IconData> IconMap = new Dictionary<StatusDataKind, IconData>()
        {
            {StatusDataKind.Nothing, new IconData(null)},
            {StatusDataKind.LoadingUI, new IconData("paint_brush_regular")},
            {StatusDataKind.LoadingData, new IconData("arrow_download_regular")},
            {StatusDataKind.SendingGrpcRequest, new IconData("globe_clock_regular")},
            {StatusDataKind.HandlingGrpcResponse, new IconData("protocol_handler_regular")},
            {StatusDataKind.Error, new IconData("error_circle_regular")},
            {StatusDataKind.Ok, new IconData("checkmark_regular")},

        };
        private static readonly StatusData _statusDataNothing = new StatusData(String.Empty, StatusDataKind.Nothing);
        public StatusData(string message, StatusDataKind kind)
        {
            Message = message;
            Kind = kind;
        }

        public static StatusData Nothing() => _statusDataNothing;
        public static StatusData LoadingUI(string message) => new StatusData(message, StatusDataKind.LoadingUI);
        public static StatusData LoadingData(string message) => new StatusData(message, StatusDataKind.LoadingData);
        public static StatusData SendingGrpcRequest(string message) => new StatusData(message, StatusDataKind.SendingGrpcRequest);
        public static StatusData HandlingGrpcResponse(string message) => new StatusData(message, StatusDataKind.HandlingGrpcResponse);
        public static StatusData Error(string message) => new StatusData(message, StatusDataKind.Error);
        public static StatusData Error(Exception exception) => new StatusData(exception.Message, StatusDataKind.Error);
        public static StatusData Ok(string message) => new StatusData(message, StatusDataKind.Ok);

        public string Message { get; set; }
        public StatusDataKind Kind { get; set; }

        public StreamGeometry Icon
        {
            get
            {
                if(Kind == StatusDataKind.Nothing)
                {
                    return null;
                }
                if (App.Current.TryFindResource(IconMap[Kind].Key, out var icon))
                {
                    IconMap[Kind].Icon = (StreamGeometry)icon;
                    return (StreamGeometry)icon;
                }
                return null;
            }
        }

    }
}
