using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerConfig _playerConfig; 
    [SerializeField] private MobileInput _mobileInput;
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private BoostTimerUI _boostTimerUI;

    [Header("Internet Checker")]
    [SerializeField] private float _internetCheckInterval = 60f;

    [Header("Effects Settings")]
    [SerializeField] private ParticleSystem _dieEffectPrefab;
    [SerializeField] private Transform _effectsParent;
    [SerializeField] private int _dieEffectPoolSize = 10;
    
    private Camera _camera => Camera.main;

    public override void InstallBindings()
    {
        GameObject runnerGameObject = new GameObject("CoroutineRunner");
        CoroutineRunnerMonoBehaviour runner = runnerGameObject.AddComponent<CoroutineRunnerMonoBehaviour>();
        Container.Bind<ICoroutineRunner>().FromInstance(runner).AsSingle();
        
        //Container.Bind<ICoroutineRunner>().To<CoroutineRunnerMonoBehaviour>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        Container.Bind<PlayerConfig>().FromInstance(_playerConfig).AsSingle();
        Container.Bind<SkinSaver>().AsSingle().NonLazy();
        Container.Bind<CurrencyService>().AsSingle().WithArguments(_playerConfig.StartingGold).NonLazy();
        
        Container.Bind<UIInfo>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<CameraControl>().FromComponentInHierarchy().AsSingle().NonLazy();

        Container.Bind<CheckPoints>().FromComponentInHierarchy().AsSingle().NonLazy();
        
        Container.BindInterfacesAndSelfTo<Player>().FromComponentInNewPrefab(_player).AsSingle().NonLazy();
        Container.Bind<PlayerSkin>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<Camera>().FromInstance(_camera).AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<GameManager>().FromComponentInHierarchy().AsSingle().NonLazy();

        if (Application.isMobilePlatform)
        {
            _mobileInput.Activate();
            Container.BindInterfacesAndSelfTo<MobileInput>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
        else
        {
            Container.BindInterfacesAndSelfTo<DesktopInput>().AsSingle().NonLazy();
        }

        Container.Bind<ProgressBar>().FromInstance(_progressBar).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<UIController>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<BoostTimerUI>().FromInstance(_boostTimerUI).AsSingle().NonLazy();

        Container.Bind<Restart>().FromComponentInHierarchy().AsSingle().NonLazy();
       
        Container.Bind<ParticleSystem>().FromInstance(_dieEffectPrefab).AsSingle().WhenInjectedInto<DeathEffect>();
        Container.Bind<int>().FromInstance(_dieEffectPoolSize).AsSingle().WhenInjectedInto<DeathEffect>();
        Container.Bind<Transform>().FromInstance(_effectsParent).AsSingle().WhenInjectedInto<DeathEffect>();
        Container.Bind<DeathEffect>().AsSingle().NonLazy();

        Container.Bind<float>().FromInstance(_internetCheckInterval).AsSingle().WhenInjectedInto<InternetConnectionChecker>();
        Container.BindInterfacesAndSelfTo<InternetConnectionChecker>().AsSingle().NonLazy();
    }
}