using Zenject;

public class TestMessageInstaller : MonoInstaller
{
    public NetworkMessageChatLabel ChatLabel;

    public override void InstallBindings()
    {
        Container.BindInterfacesTo<NetworkMessageService>().AsSingle().NonLazy();
        Container.Bind<IChatView>().FromInstance(ChatLabel).AsSingle();
    }
}
