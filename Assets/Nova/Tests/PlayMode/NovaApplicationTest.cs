using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using Nova.Framework.Core;
using Nova.Framework.Dependency;
using Nova.Framework.Controller;
using Nova.Framework.Entity;
using Nova.Framework.Entity.Component;

namespace Nova.Framework.Tests
{
    public class NovaApplicationTest
    {
        [UnityTest]
        public IEnumerator Should_Create_Application_Then_Start_Component()
        {
            yield return new MonoBehaviourTest<SampleNovaGameApplication>();
        }

        private class SampleApplicationResult
        {
            public bool onPreLoad, onLoad, onAwake, onStart, onUpdate, onLateUpdate, onDestroy, onEnable, onDisable = false;
            public bool AllTrue => onPreLoad && onLoad && onAwake && onStart && onUpdate && onLateUpdate && onEnable && onDisable && onDestroy;
        }

        private interface IWeatherController : IController { }

        private class WeatherController : IWeatherController { }

        private class SampleNovaGameApplication : MonoBehaviour, IMonoBehaviourTest
        {
            private INovaFrameworkBuilder _application;
            private GameObject _worldComponentGO;
            private WorldComponent_Entity[] _worldComponentEntities;
            private SampleApplicationResult[] _sampleApplicationResults;

            public bool IsTestFinished =>
                   _sampleApplicationResults != null
                && _sampleApplicationResults[0] != null
                && _sampleApplicationResults[0].AllTrue
                && _sampleApplicationResults[1] != null
                && _sampleApplicationResults[1].AllTrue;

            public void Awake()
            {
                _application = new NovaApplication();

                _application.WithSettings(options =>
                {
                    options.AddSingleton(typeof(IWeatherController), typeof(WeatherController));
                });

                _application.Start();
            }

            public void Start()
            {
                _worldComponentGO = new GameObject();

                _worldComponentEntities = new WorldComponent_Entity[2];
                _sampleApplicationResults = new SampleApplicationResult[2];

                _worldComponentEntities[0] = _worldComponentGO.AddComponent<WorldComponent_Entity>();
                _sampleApplicationResults[0] = _worldComponentEntities[0].sampleApplicationResult;

                _worldComponentEntities[1] = _worldComponentGO.AddComponent<WorldComponent_Entity>();
                _sampleApplicationResults[1] = _worldComponentEntities[1].sampleApplicationResult;

                StartCoroutine(DestroyAfterSeconds(0.1f));
            }

            public IEnumerator DestroyAfterSeconds(float seconds)
            {
                yield return null;

                yield return new WaitForSeconds(seconds);

                _worldComponentEntities[0].enabled = false;
                _worldComponentEntities[1].enabled = false;

                yield return null;

                yield return new WaitForSeconds(seconds);

                Destroy(_worldComponentGO);

                _worldComponentGO = null;

                yield return null;
            }

            public void OnDestroy()
            {
                _application.Dispose();
            }
        }

        private class WorldComponent_Entity : EntityTickableBase
        {
            public SampleApplicationResult sampleApplicationResult = new SampleApplicationResult();

            public override IComponent[] GetComponents()
            {
                return new[] { new WorldComponent(sampleApplicationResult) };
            }
        }

        private interface IWorldComponent : IComponent { }

        private class WorldComponent : IWorldComponent, IStartable, IUpdateable, IDestroyable, IEnableable, ILoadable
        {
            private SampleApplicationResult _sampleApplicationResult;

            public WorldComponent(SampleApplicationResult sampleApplicationResult)
            {
                _sampleApplicationResult = sampleApplicationResult;
            }

            void ILoadable.OnPreLoad(IDependencyContainer container) => _sampleApplicationResult.onPreLoad = true;

            void ILoadable.OnLoad(IDependencyContainer container) => _sampleApplicationResult.onLoad = true;

            void IStartable.OnAwake() => _sampleApplicationResult.onAwake = true;

            void IStartable.OnStart() => _sampleApplicationResult.onStart = true;

            void IUpdateable.OnUpdate() => _sampleApplicationResult.onUpdate = true;

            void IUpdateable.OnLateUpdate() => _sampleApplicationResult.onLateUpdate = true;

            void IEnableable.OnEnable() => _sampleApplicationResult.onEnable = true;

            void IEnableable.OnDisable() => _sampleApplicationResult.onDisable = true;

            void IDestroyable.OnDestroy() => _sampleApplicationResult.onDestroy = true;
        }
    }
}