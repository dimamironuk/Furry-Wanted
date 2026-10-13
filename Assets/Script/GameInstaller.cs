using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Header("Player & Core Systems")]
    [SerializeField] private AnimationPlayer animationPlayer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private HPController hpController;
    [SerializeField] private CameraController cameraController;

    [Header("Dialogue System")]
    [SerializeField] private DialogSystem dialogSystem;

    public override void InstallBindings()
    {
        Container.Bind<BookController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<HPController>().FromInstance(hpController).AsSingle();
        Container.Bind<CameraController>().FromInstance(cameraController).AsSingle();
        Container.Bind<InventoryManager>().FromInstance(inventoryManager).AsSingle();
        Container.Bind<AnimationPlayer>().FromInstance(animationPlayer).AsSingle();
        Container.Bind<PlayerController>().FromInstance(playerController).AsSingle();

        Container.Bind<IInputReader>().To<KeyboardInputReader>().AsSingle();

        Container.BindInterfacesAndSelfTo<DialogSystem>()
            .FromInstance(dialogSystem)
            .AsSingle();
    }
}