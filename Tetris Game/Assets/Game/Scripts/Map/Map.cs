using DG.Tweening;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [System.NonSerialized] private Coroutine _shootRoutine = null;
        [System.NonSerialized] private Coroutine _mainRoutine = null;

        public void StartMainLoop()
        {
            _mainRoutine = StartCoroutine(MainLoop());
        }

        private IEnumerator MainLoop()
        {
            float prevShoot = Time.time;
            // bool _canShoot = true;
            
            _shootRoutine = StartCoroutine(ShootRoutine());
            IEnumerator ShootRoutine()
            {
                while (true)
                {
                    if (Time.time - prevShoot > 1.5f)
                    {
                        // if (_canShoot)
                        {
                            Board.THIS.GiveBullet();
                            prevShoot = Time.time;
                        }
                    }

                    yield return null;
                }
            }
            
            while (true)
            {
                Board.THIS.MoveAll(0.25f);
                yield return new WaitForSeconds(0.3f);
                Board.THIS.CheckSteady();
                //
                // List<int> tetrisLines = Grid.THIS.CheckTetris();
                //
                // if (tetrisLines.Count > 0)
                // {
                //     if (tetrisLines.Count > 1)
                //     {
                //         UIManager.THIS.ft_Combo.FlyScreen("x" + tetrisLines.Count, Vector3.zero, 0.0f);
                //         yield return new WaitForSeconds(0.4f);
                //     }
                //
                //     // _canShoot = false;
                //     
                //     Grid.THIS.MergeLines(tetrisLines, 0.2f);
                //
                //     Grid.THIS.MarkNewMovers(tetrisLines[0]);
                //     yield return new WaitForSeconds(0.75f);
                // }
                //
                // yield return new WaitForSeconds(0.15f);
                // _canShoot = true;
            }
        }
        
        public void Deconstruct()
        {
            if (_shootRoutine != null)
            {
                StopCoroutine(_shootRoutine);
                _shootRoutine = null;
            }
            if (_mainRoutine != null)
            {
                StopCoroutine(_mainRoutine);
                _mainRoutine = null;
            }
        }
    }
}
