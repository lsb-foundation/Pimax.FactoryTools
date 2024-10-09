using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Pimax.FactoryTool.KingdeePrinter.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            WeakReferenceMessenger.Default.Register<AppMessage>(this, ShowAppMessage);
            ShowAppMessage(this, new AppMessage(AppMessageType.Info, "Ready!"));
        }

        private string appMessage;
        public string AppMessage
        {
            get => appMessage;
            set => SetProperty(ref appMessage, value);
        }

        private AppMessageType messageType;
        public AppMessageType MessageType
        {
            get => messageType;
            set => SetProperty(ref messageType, value);
        }

        private void ShowAppMessage(object recipient, AppMessage message)
        {
            AppMessage = message.Content;
            MessageType = message.Type;
        }
    }

    public class AppMessage : ObservableObject
    {
        public string Content { get; }

        public AppMessageType Type { get; }

        public AppMessage(AppMessageType type, string message)
        {
            Type = type;
            Content = message;
        }

        public static void Show(AppMessageType type, string message)
        {
            WeakReferenceMessenger.Default.Send(new AppMessage(type, message));
        }

        public static void Clear()
        {
            WeakReferenceMessenger.Default.Send(new AppMessage(AppMessageType.Info, string.Empty));
        }
    }

    public enum AppMessageType
    {
        Succeed,
        Info,
        Error
    }
}
