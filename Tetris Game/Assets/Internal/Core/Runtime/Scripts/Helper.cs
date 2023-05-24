using System;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Internal.Core
{
    [Serializable]
    public struct PosRotData
    {
        [SerializeField] public Vector3 pos;
        [SerializeField] public Vector3 rot;
    }

    [System.Serializable]
    public class GenericData
    {
        public virtual bool CanAct()
        {
            return true;
        }
    }

    public static class Helper
    {
        public static void SetChildLayers(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            Transform transform = gameObject.transform;
            foreach (Transform child in transform)
            {
                child.gameObject.SetChildLayers(layer);
            }
        }
        public static T[] Fill<T>(this T[] array, T data)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = data;
            }
            return array;
        }
        public static T At<T>(this T[] t, int index)
        {
            int finalIndex = Mathf.Clamp(index, 0, t.Length - 1);
            return t[finalIndex];
        }
        public static void LogE(this object obj)
        {
            Debug.LogError(obj.ToString());
        }
        public static void LogW(this object obj)
        {
            Debug.LogWarning(obj.ToString());
        }
        public static void Log(this object obj)
        {
            Debug.Log(obj.ToString());
        } 
        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
        public static void Show(this GameObject gameObject, bool state, float duration = 0.25f, float magnitude = 1.0f)
        {
            gameObject.transform.DOKill();
            gameObject.transform.DOScale(Vector3.one * (state ? magnitude : 0.0f), duration).SetEase(state ? Ease.OutBack : Ease.InBack);
        }
        public static void Show(this Transform transform, bool state, float duration = 0.25f, float magnitude = 1.0f)
        {
            transform.DOKill();
            transform.DOScale(Vector3.one * (state ? magnitude : 0.0f), duration).SetEase(state ? Ease.OutBack : Ease.InBack);
        }
        public static void PunchRight(this RectTransform rectTransform, float magnitude, float duration)
        {
            rectTransform.DOKill();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.DOPunchPosition(Vector3.right * magnitude, duration);
        }
        public static void PunchUp(this RectTransform rectTransform, float magnitude, float duration)
        {
            rectTransform.DOKill();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.DOPunchPosition(Vector3.up * magnitude, duration);
        }
        public static void PunchUp(this TextMeshProUGUI text, float magnitude, float duration)
        {
            PunchUp(text.rectTransform, magnitude, duration);
        }
        public static void CopyFrom<T>(this List<T> copyTo, List<T> copyFrom) where T : class, ICloneable
        {
            copyFrom.ForEach((item) =>
            {
                copyTo.Add(item.Clone() as T);
            });
        }
        public static void CopyTo<T>(this List<T> copyFrom, List<T> copyTo) where T : class, ICloneable
        {
            copyFrom.ForEach((item) =>
            {
                copyTo.Add(item.Clone() as T);
            });
        }
        public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
        {
            Vector2 localPoint;
            Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * 0.5f + from.rect.xMin, from.rect.height * 0.5f + from.rect.yMin);
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
            screenP += fromPivotDerivedOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
            Vector2 pivotDerivedOffset = new Vector2(to.rect.width * 0.5f + to.rect.xMin, to.rect.height * 0.5f + to.rect.yMin);
            return to.anchoredPosition + localPoint - pivotDerivedOffset;
        }
        public static Vector3 ToScreenPosition(this Vector3 worldPosition, Camera rendererCamera, Canvas canvas)
        {
            Vector2 viewport = rendererCamera.WorldToViewportPoint(worldPosition);
            Ray canvasRay = canvas.worldCamera.ViewportPointToRay(viewport);
            return canvasRay.GetPoint(canvas.planeDistance);

            //Vector3 screenPoint = rendererCamera.WorldToScreenPoint(worldPosition);
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, canvas.worldCamera, out Vector2 result);
            //return result;
        }
        public static Vector3 ToScreenPosition(this Vector3 worldPosition, Camera rendererCamera, Camera canvasCamera, float distance)
        {
            Vector2 viewport = rendererCamera.WorldToViewportPoint(worldPosition);
            Ray canvasRay = canvasCamera.ViewportPointToRay(viewport);
            return canvasRay.GetPoint(distance);
        }
        public static Tween DoAlpha(this TextMeshProUGUI text, float targetAlpha, float duration, Ease ease, float? startAlpha = null)
        {
            Color currentColor = text.color;
            currentColor.a = startAlpha != null ? (float)startAlpha : currentColor.a;

            float value = 0.0f;
            Tween tween = DOTween.To((x) => value = x, currentColor.a, targetAlpha, duration).SetEase(ease);
            tween.onUpdate = () =>
            {
                currentColor.a = value;
                text.color = currentColor;
            };
            return tween;
        }
        public static Tween DoAlpha(this CanvasGroup canvasGroup, float targetAlpha, float duration, Ease ease, System.Action OnEnd = null)
        {
            float value = 0.0f;
            Tween tween = DOTween.To((x) => value = x, canvasGroup.alpha, targetAlpha, duration).SetEase(ease);
            tween.onUpdate = () =>
            {
                canvasGroup.alpha = value;
            };
            tween.onComplete = () =>
            {
                OnEnd?.Invoke();
            };
            return tween;
        }
        public static string ToTMProKey(this string key)
        {
            return "<sprite name=\"" + key + "\">";
        }
        public static void SetAlpha(this Graphic graphics, float alpha)
        {
            Color color = graphics.color;
            color.a = alpha;
            graphics.color = color;
        }
        public static Vector3 GetForwardIgnoreY(Vector3 start, Vector3 end)
        {
            start.y = 0.0f;
            end.y = 0.0f;
            return (end - start).normalized;
        }
        public static Tween DoColor(this Renderer renderer, MaterialPropertyBlock mpb, string key, Color fromColor, Color toColor, float duration, Ease ease)
        {
            float timeStep = 0.0f;
            Tween tween = DOTween.To((x) => timeStep = x, 0.0f, 1.0f, duration).SetEase(ease);
            tween.onUpdate = () =>
            {
                Color currentColor = Color.Lerp(fromColor, toColor, timeStep);
                renderer.SetColor(mpb, key, currentColor);
            };
            return tween;
        }
        public static Tween AnimateShaderColor(this Renderer renderer, MaterialPropertyBlock mpb, string key, Color fromColor, Color toColor, float duration, Ease ease, params int[] indexes)
        {
            float timeStep = 0.0f;
            Tween tween = DOTween.To((x) => timeStep = x, 0.0f, 1.0f, duration).SetEase(ease);
            tween.onUpdate = () =>
            {
                Color currentColor = Color.Lerp(fromColor, toColor, timeStep);

                for (int i = 0; i < indexes.Length; i++)
                {
                    renderer.SetColor(mpb, key, currentColor, i);
                }
            };
            return tween;
        }
        public static Color32 GetSRGBColor(this int key)
        {
            byte red = ((byte)(key % 255));
            byte green = ((byte)(key / 255));
            byte blue = 0;
            byte alpha = 255;
            return new Color32(red, green, blue, alpha);
        }
        public static int GetIndexFromSRGBColor(this Color32 color)
        {
            int red = color.r;
            int green = color.g * 255;
            int blue = 0;
            return red + green + blue;
        }
        public static void FitInsideParent(this Transform transform, Transform parent)
        {
            transform.SetParent(parent);
            transform.Fit();
        }
        public static void Fit(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        public static RenderTexture CreateRenderTexture(this Camera cam, out RenderTexture renderTexture, Vector2Int screenSize, RenderTextureFormat textureFormat, FilterMode filterMode)
        {
            renderTexture = new RenderTexture(screenSize.x, screenSize.y, 0, textureFormat, 0);
            renderTexture.filterMode = filterMode;
            cam.targetTexture = renderTexture;
            return renderTexture;
        }
        public static void SetAllActive(this List<GameObject> list, bool state)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].SetActive(state);
            }
        }
        public static void SetAllActive(this List<List<GameObject>> list, bool state)
        {
            foreach (var item in list)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] != null)
                    {
                        item[i].SetActive(state);
                    }
                }
            }
        }
        public static void SetAllActive(this List<List<GameObject>> list, HashSet<GameObject> alwaysFalseList, bool state)
        {
            foreach (var item in list)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] != null)
                    {
                        bool finalState = state & !alwaysFalseList.Contains(item[i]);
                        item[i].SetActive(finalState);
                    }
                }
            }
        }
        public static Vector3 RandomXZ(float radius)
        {
            return new Vector3(UnityEngine.Random.Range(-radius, radius), 0.0f, UnityEngine.Random.Range(-radius, radius));
        }
        public static Vector3 RandomVerticalRotation()
        {
            return new Vector3(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
        }
        public static Vector3 RandomScale(float min, float max)
        {
            return Vector3.one * UnityEngine.Random.Range(min, max);
        }
        public static Tween GoFade(this MaskableGraphic image, float targetAlpha, float duration, Ease ease, bool isIndependentUpdate, float delay, float? startAlpha = null, System.Action onEndCallback = null)
        {
            Color color = image.color;

            if (startAlpha != null)
            {
                color.a = (float)startAlpha;
                image.color = color;
            }

            float timeStep = 0.0f;
            Tween tween = DOTween.To((x) => timeStep = x, color.a, targetAlpha, duration).SetEase(ease).SetDelay(delay).SetUpdate(isIndependentUpdate);
            tween.onUpdate = () =>
            {
                color.a = timeStep;
                image.color = color;
            };
            tween.onComplete = () =>
            {
                onEndCallback?.Invoke();
            };
            return tween;
        }
        public static Vector3? RayPlanePoint(this Camera cam, float height)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Plane plane = new Plane(Vector3.up, new Vector3(0.0f, height, 0.0f));

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                return hitPoint;
            }
            return null;
        }
        public static Color Random(this Gradient gradient)
        {
            return gradient.Evaluate(UnityEngine.Random.Range(0.0f, 1.0f));
        }
        public static Color GetRandomColor()
        {
            return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        }
        public static void Mimic(this Camera targetCamera, Camera copyFrom)
        {
            targetCamera.orthographic = copyFrom.orthographic;
            targetCamera.fieldOfView = copyFrom.fieldOfView;
            targetCamera.orthographicSize = copyFrom.orthographicSize;
            targetCamera.nearClipPlane = copyFrom.nearClipPlane;
            targetCamera.farClipPlane = copyFrom.farClipPlane;
        }
        public static void MimicFov(this Camera targetCamera, Camera copyFrom)
        {
            targetCamera.fieldOfView = copyFrom.fieldOfView;
        }
        public static void Mimic(this Transform thisTransform, Transform targetTransform)
        {
            targetTransform.position = thisTransform.position;
            targetTransform.rotation = thisTransform.rotation;
            targetTransform.localScale = thisTransform.localScale;
        }
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector3 position = corners[0];

            Vector2 size = new Vector2(
                rectTransform.lossyScale.x * rectTransform.rect.size.x,
                rectTransform.lossyScale.y * rectTransform.rect.size.y);

            return new Rect(position, size);
        }
        public static Rect GetScreenPositionFromRect(this RectTransform rt, Camera camera)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            for (var i = 0; i < corners.Length; i++)
                corners[i] = camera.WorldToScreenPoint(corners[i]);

            var position = (Vector2)corners[1];
            position.y = Screen.height - position.y;
            var size = corners[2] - corners[0];

            return new Rect(position, size);
        }
        public static Rect ScreenPosition(this RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            return new Rect((Vector2)transform.position - (size * 0.5f), size);
        }
        public static void WriteToFile(RenderTexture renderTexture, string path)
        {
            WriteToFile(renderTexture.ToTexture2D(), path);
        }
        public static void WriteToFile(Texture2D texture2D, string path)
        {
            byte[] bytes = texture2D.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
        }
        public static Texture2D ToTexture2D(this RenderTexture renderTexture)
        {
            Texture2D tex = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }

        public static Vector3 BallisticVelocityVector(Vector3 initialPos, Vector3 finalPos, float angle, Vector3 gravity)
        {
            var toPos = initialPos - finalPos;

            var h = toPos.y;

            toPos.y = 0;
            var r = toPos.magnitude;

            var g = -gravity.y;

            var a = Mathf.Deg2Rad * angle;

            var vI = Mathf.Sqrt(((Mathf.Pow(r, 2f) * g)) / (r * Mathf.Sin(2f * a) + 2f * h * Mathf.Pow(Mathf.Cos(a), 2f)));

            Vector3 velocity = (finalPos - initialPos).normalized * Mathf.Cos(a);
            velocity.y = Mathf.Sin(a);

            return velocity * vI;
        }
        public static void SetFloat(this Renderer renderer, MaterialPropertyBlock mpb, string key, float value)
        {
            renderer.GetPropertyBlock(mpb, 0);
            mpb.SetFloat(key, value);
            renderer.SetPropertyBlock(mpb, 0);
        }
        public static void SetVector(this Renderer renderer, MaterialPropertyBlock mpb, string key, Vector2 value)
        {
            renderer.GetPropertyBlock(mpb, 0);
            mpb.SetVector(key, value);
            renderer.SetPropertyBlock(mpb, 0);
        }
        public static void SetColor(this SpriteRenderer renderer, MaterialPropertyBlock mpb, string key, Color value)
        {
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor(key, value);
            renderer.SetPropertyBlock(mpb);
        }
        public static void SetColor(this Renderer renderer, MaterialPropertyBlock mpb, string key, Color value, int matIndex = 0)
        {
            renderer.GetPropertyBlock(mpb, matIndex);
            mpb.SetColor(key, value);
            renderer.SetPropertyBlock(mpb, matIndex);
        }
        public static void SetTexture(this Renderer renderer, MaterialPropertyBlock mpb, string key, Texture value)
        {
            renderer.GetPropertyBlock(mpb, 0);
            mpb.SetTexture(key, value);
            renderer.SetPropertyBlock(mpb, 0);
        }
        public static void SetImageSize(Image image)
        {
            float orgX = image.rectTransform.rect.size.x;
            Vector2 imageSize = image.sprite.rect.size;
            float ratio = imageSize.x / imageSize.y;
            image.rectTransform.sizeDelta = new Vector2(orgX, orgX / ratio);
        }
        public static void SetImageSize(RawImage rawImage)
        {
            float orgX = rawImage.rectTransform.rect.size.x;
            Vector2 imageSize = new Vector2(rawImage.texture.width, rawImage.texture.height);
            float ratio = imageSize.x / imageSize.y;
            rawImage.rectTransform.sizeDelta = new Vector2(orgX, orgX / ratio);
        }

        //public static Vector3 SplinePercent2Position(SplineComputer splineComputer, double percent)
        //{
        //    return splineComputer.Evaluate(percent).position;
        //}
        //public static SplineSample SplinePercent2Sample(SplineComputer splineComputer, double percent)
        //{
        //    return splineComputer.Evaluate(percent);
        //}
        public static bool IsPossible(this float chance)
        {
            bool possible = UnityEngine.Random.Range(0.0f, 1.0f) < chance;
            return possible;
        }
        public static bool IsPossible(this float chance, float max)
        {
            bool possible = UnityEngine.Random.Range(0.0f, max) < chance;
            return possible;
        }
        public static bool NotInRange(this List<Vector3> list, Vector3 position, float distance)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Vector3 point = list[i];
                if ((point - position).magnitude <= distance)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Possible(float chance)
        {
            bool possible = UnityEngine.Random.Range(0.0f, 1.0f) < chance;
            return possible;
        }
        public static bool Possible(float chance, float max)
        {
            bool possible = UnityEngine.Random.Range(0.0f, max) < chance;
            return possible;
        }

        public static T Random<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        public static T Random<T>(this T[] list)
        {
            return list[UnityEngine.Random.Range(0, list.Length)];
        }
        public static T RandomRemove<T>(this List<T> list)
        {
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            T t = list[randomIndex];
            list.RemoveAt(randomIndex);
            return t;
        }
        public delegate void DespawnAction(GameObject gameObject, float t = 0);
        public static void KillChildTweensAndDespawn(this Transform transform, DespawnAction OnDespawnAction)
        {
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                child.DOKill();
                OnDespawnAction.Invoke(child.gameObject);
            }
        }
        public static void KillChildTweens(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.DOKill();
            }
        }
        public static void KillChildTweensAndDestroy(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                child.DOKill();
                GameObject.Destroy(child.gameObject);
            }
        }
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        public delegate void OnChildDestroy(Transform t);
        public delegate bool CanDestroy(Transform t);
        public static void DestoryChildsBreak(this Transform transform, CanDestroy OnCanDestroy, OnChildDestroy OnChildDestroy)
        {
            foreach (Transform child in transform)
            {
                if (OnCanDestroy.Invoke(child))
                {
                    OnChildDestroy.Invoke(child);
                    GameObject.Destroy(child.gameObject);
                    break;
                }
            }
        }
        public static void DecreaseClamped(this ref int value)
        {
            value = Mathf.Clamp(value - 1, 0, int.MaxValue);
        }
    }


}
