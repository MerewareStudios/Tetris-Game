using System;

namespace HomaGames.Geryon
{
    // Any public API changes should be also mirrored to the Observable class defined in the script template:
    // Core/Geryon/Editor/Resources/Geryon/DvrScriptTemplate_2.txt
    public class Observable<T>
    {
        private event Action<T> ValueChanged;

        private T _value;

        public T Value
        {
            get => _value;
            internal set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                ValueChanged?.Invoke(_value);
            }
        }

        public Observable()
        {
        }

        public Observable(T value)
        {
            Value = value;
        }

        public static implicit operator T(Observable<T> obj)
        {
            return obj.Value;
        }

        public void Subscribe(Action<T> onValueChanged, bool notifyOnSubscribe = true)
        {
            if (onValueChanged == null)
                return;

            ValueChanged += onValueChanged;
            if (notifyOnSubscribe)
                onValueChanged.Invoke(_value);
        }

        public void Unsubscribe(Action<T> callback)
        {
            ValueChanged -= callback;
        }
    }
}