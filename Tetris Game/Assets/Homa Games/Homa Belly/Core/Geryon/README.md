# HOMA N-Testing (Geryon)
> N-Testing or A/B/n Testing

## Migration to DVR 2.0
The biggest and most important change from version 1.0 to 2.0 is the new DVR API.
The primitive values like `bool` or `int` that were previously returned by DVR properties are now replaced with their `Observable<T>` counterparts.
This allows the user to listen for any value changes of a property.

**Example**
```csharp
DVR.ChestRoomFrequency.Subscribe(OnChestRoomFrequencyChange);

void OnChestRoomFrequencyChange(int newChestRoomFrequency)
{
    Debug.Log($"The ChestRoomFrequency was changed, refreshing UI.");
    RefreshUI();
}
```

**IMPORTANT**
For game to properly support segmentation it has to take into account that values in the DVR class can change during gameplay.

### How to migrate
1. Open N-Testing settings view: _Edit > Project Settings > HOMA > N Testing_
2. Change the DVR Script Version to 2.0

![DVRSettings](./Documentation~/DVRSettings.png)