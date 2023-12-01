using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Airplane : MonoBehaviour
{
    [SerializeField] private Transform thisTransform;
    [SerializeField] private Transform startPivot;
    [SerializeField] private Transform target;
    [SerializeField] private Transform cargoParent;
    [SerializeField] private float altitude = 28.0f;
    [SerializeField] private ParticleSystem leftEnginePS;
    [SerializeField] private ParticleSystem rightEnginePS;
    [SerializeField] private Pool[] cargoType2Pool;
    [SerializeField] private Canvas getCanvas;
    [SerializeField] private Button getButton;
    [System.NonSerialized] private Cargo _currentCargo = null;
    [System.NonSerialized] private Tween _delayedDisable;
    
    [System.NonSerialized] private Airplane.Data _savedData;

    public Airplane.Data SavedData
    {
        set
        {
            _savedData = value;

            int cargoCount = SavedData.Count;
            DOVirtual.DelayedCall(0.15f, () =>
            {
                for (int i = 0; i < cargoCount; i++)
                {
                    SpawnCargo(SavedData.cargoTypes[i]);
                }
                
                if (SavedData.Has)
                {
                    DOVirtual.DelayedCall(0.1f * cargoCount, ShowButton);
                }
                
            }, false);
            
        }
        get => _savedData;
    }

    public void UpdatePositions()
    {
        Vector3 targetPosition = target.position;
        cargoParent.position = targetPosition;
        getCanvas.transform.position = targetPosition;
    }

    private void ShowButton()
    {
        if (getCanvas.gameObject.activeSelf)
        {
            return;
        }

        
        getButton.targetGraphic.raycastTarget = false;
        getButton.transform.localScale = Vector3.zero;
                    
        getButton.transform.DOKill();
        Tween tween = getButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        tween.OnStart(() =>
        {
            getCanvas.gameObject.SetActive(true);
        });
        tween.onComplete = () =>
        {
            getButton.targetGraphic.raycastTarget = true;
        };
    }

    private void HideButton()
    {
        getButton.transform.DOKill();
        getCanvas.gameObject.SetActive(false);
    }

    public void OnClick_Get()
    {
        if (!SavedData.Has)
        {
            return;
        }

        SavedData.UnpackCargo(SavedData.Count - 1);
        
        if (_currentCargo != null)
        {
            _currentCargo.Redrop();
            return;
        }
        
        if (!SavedData.Has)
        {
            HideButton();
        }
    }

    public void CarryCargo(CarryData carryData)
    {
        this.gameObject.SetActive(true);
        thisTransform.DOKill();
        _delayedDisable?.Kill();
        
        Vector3 startPosition = startPivot.position + Vector3.forward * Random.Range(-7.0f, 7.0f);
        startPosition.y = altitude;
        Vector3 targetPosition = target.position;
        targetPosition.y = altitude;
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 endPosition = targetPosition + direction * 4.0f;

        thisTransform.position = startPosition;
        thisTransform.forward = direction;

        Travel(targetPosition, 5.0f, Ease.OutSine).SetDelay(carryData.delay).onComplete = () =>
        {
            Travel(endPosition, 12.0f, SavedData.Full ? Ease.InBack : Ease.InSine).onComplete = () =>
            {
                _delayedDisable = DOVirtual.DelayedCall(0.6f, () =>
                {
                    this.gameObject.SetActive(false);
                }, false);
            };
            if (SavedData.Full)
            {
                UIManager.THIS.speechBubble.Speak(Onboarding.THIS.airDropFull, 0.0f, 1.5f);
                return;
            }

            switch (carryData.type)
            {
                case Cargo.Type.MaxStack:
                    UIManager.THIS.speechBubble.Speak(Onboarding.THIS.maxStackDropCheer, 0.25f, 1.5f);
                    break;
                case Cargo.Type.Health:
                    UIManager.THIS.speechBubble.Speak(Onboarding.THIS.healthDropCheer, 0.25f, 1.5f);
                    break;
            }

            DropCargo(carryData.type);
        };
        
        leftEnginePS.Clear();
        leftEnginePS.Play();
        rightEnginePS.Clear();
        rightEnginePS.Play();
    }

    private void DropCargo(Cargo.Type type)
    {
        _currentCargo = cargoType2Pool[(int)type].Spawn<Cargo>();
        _currentCargo.Drop(cargoParent, altitude - 0.75f, () =>
        {
            SavedData.AddCargo(_currentCargo);
            SavedData.arrival = LevelManager.CurrentLevel;
            if (SavedData.Has)
            {
                ShowButton();
            }

            _currentCargo = null;
        });
    }

    private void SpawnCargo(Cargo.Type type)
    {
        Cargo cargo = cargoType2Pool[(int)type].Spawn<Cargo>();
        cargo.Place(cargoParent);
        
        SavedData.Cargoes.Add(cargo);
    }


    private Tween Travel(Vector3 toPosition, float speed, Ease ease)
    {
        return thisTransform.DOMove(toPosition, speed).SetSpeedBased(true).SetEase(ease, 4.0f);
    }

    public void OnDeconstruct()
    {
        this.gameObject.SetActive(false);
        thisTransform.DOKill();
        _delayedDisable?.Kill();
    }

    [System.Serializable]
    public class CarryData
    {
        [SerializeField] public Cargo.Type type;
        [SerializeField] public int delay = -1;
    }


    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public List<Cargo.Type> cargoTypes;
        [SerializeField] public int arrival = -1;
        [System.NonSerialized] public List<Cargo> Cargoes = new();
            
        public Data()
        {
            
        }
        public Data(Data data)
        {
            this.cargoTypes = new List<Cargo.Type>(data.cargoTypes);
            this.arrival = data.arrival;
        }

        public void AddCargo(Cargo cargo)
        {
            Cargoes.Add(cargo);
            cargoTypes.Add(cargo.type);
        }
        
        public void UnpackCargo(int index)
        {
            Cargoes[index].Unpack();
            Cargoes.RemoveAt(index);
            cargoTypes.RemoveAt(index);
        }

        public int Count => cargoTypes.Count;
        public bool Full => Count >= 5;
        public bool Has => Count > 0;

        public object Clone()
        {
            return new Data(this);
        }
    } 
}
