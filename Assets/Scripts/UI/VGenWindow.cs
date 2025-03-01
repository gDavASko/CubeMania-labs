using System;
using System.Linq;
using GDB.Planetary;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace GDB.UI
{
    public class VGenWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _mainBlockType = null;
        [SerializeField] private TMP_Dropdown _destroyableBlockType = null;
        [SerializeField] private TMP_Dropdown _nondestroyableBlockType = null;
        [SerializeField] private TMP_Dropdown _resourcesBlockType = null;

        [SerializeField] private TMP_InputField _centers = null;
        [SerializeField] private TMP_InputField _maxRadius = null;
        [SerializeField] private TMP_InputField _minNeighbors = null;
        [SerializeField] private TMP_InputField _maxNeighbors = null;

        [SerializeField] private TMP_InputField _resCount = null;
        [SerializeField] private TMP_InputField _minResNeighbors = null;
        [SerializeField] private TMP_InputField _maxResNeighbors = null;
        [SerializeField] private TMP_InputField _maxResBlocks = null;

        [SerializeField] private Button _btnGenerate = null;

        void Start()
        {
            _btnGenerate.onClick.AddListener(OnGenPress);
        }

        private void OnGenPress()
        {
            var MainBlockType =
                _mainBlockType.value == 0 ? CubeType.Ice : (CubeType)_mainBlockType.value;

            var DestroyableBlockType =
                _destroyableBlockType.value == 0 ? CubeType.Sand : (CubeType)_destroyableBlockType.value;

            var NonDestroyableBlockType =
                _nondestroyableBlockType.value == 0 ? CubeType.Rock : (CubeType)_nondestroyableBlockType.value;

            var ResourcesBlockType =
                _resourcesBlockType.value == 0 ? CubeType.Resource : (CubeType)_resourcesBlockType.value;


            uint centers = uint.Parse(_centers.text);
            uint maxRadius = uint.Parse(_maxRadius.text);
            uint minNeighbors = uint.Parse(_minNeighbors.text);
            uint maxNeighbors = uint.Parse(_maxNeighbors.text);
            uint resCount = uint.Parse(_resCount.text);
            uint minResNeighbors = uint.Parse(_minResNeighbors.text);
            uint maxResNeighbors = uint.Parse(_maxResNeighbors.text);
            uint maxResBlocks = uint.Parse(_maxResBlocks.text);

            RantimeGeneratorS system =
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<RantimeGeneratorS>();

            system.RegenerateCubes(new GenParams()
            {
                MainBlockType = MainBlockType,
                DestroyableBlockType = DestroyableBlockType,
                NonDestroyableBlockType = NonDestroyableBlockType,
                ResourcesBlockType = ResourcesBlockType,
                Centers = centers,
                MaxRadius = maxRadius,
                MinNeighbors = minNeighbors,
                MaxNeighbors = maxNeighbors,
                ResCount = resCount,
                MinResNeighbors = minResNeighbors,
                MaxResNeighbors = maxResNeighbors,
                MaxResBlocks = maxResBlocks
            });
        }

        private void OnValidate()
        {
            if (_mainBlockType.options.Count == 0)
            {
                _mainBlockType.AddOptions(System.Enum.GetNames(typeof(CubeType)).ToList());
                _mainBlockType.value = (int)CubeType.Ice;
            }

            if (_destroyableBlockType.options.Count == 0)
            {
                _destroyableBlockType.AddOptions(System.Enum.GetNames(typeof(CubeType)).ToList());
                _destroyableBlockType.value = (int)CubeType.Sand;
            }

            if (_nondestroyableBlockType.options.Count == 0)
            {
                _nondestroyableBlockType.AddOptions(System.Enum.GetNames(typeof(CubeType)).ToList());
                _nondestroyableBlockType.value = (int)CubeType.Rock;
            }

            if (_resourcesBlockType.options.Count == 0)
            {
                _resourcesBlockType.AddOptions(System.Enum.GetNames(typeof(CubeType)).ToList());
                _resourcesBlockType.value = (int)CubeType.Resource;
            }
        }
    }
}