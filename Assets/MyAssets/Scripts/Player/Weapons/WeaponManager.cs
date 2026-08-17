using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    [SerializeField] private GameObject weaponSocket;
    [Space]
    [SerializeField] private bool startWithEngine = false;
    [SerializeField] private GameObject enginePrefab;
    [SerializeField] private bool startWithFlareGun = false;
   

    public readonly Dictionary<WeaponID, WeaponBase> _inventory = new();
    private WeaponID _lastEquippedID;
    private WeaponBase _equippedWeapon;
    private WeaponID _equippedWeaponID = WeaponID.None;
    private WeaponID _requestedWeapon = WeaponID.NoChange;

    private bool _requestedWeaponSwap = false;
    private WeaponID _playerRequestedWeapon = WeaponID.NoChange;
    private bool _requestedActionPressed = false;
    private bool _requestedActionHeld = false;
    private bool _requestedReadyPressed = false;
    private bool _requestedReadyHeld = false;

    public void Initialise()
    {
        if (startWithEngine)
        {
            PickupWeapon(WeaponID.Engine);
        }
    }
    
    // pickup implementation
    private void PickupWeapon(WeaponID newWeaponID)
    {
        // Upon picking up a weapon-
        // instantiate it's prefab, make it a child of the WeaponSocket's transform
        // Add it to the inventory of weapons under it's weapon ID
        // Request for it to be equipped.
        switch (newWeaponID)
        {
            case WeaponID.None: break;
            case WeaponID.Engine:
                GameObject engineWeapon = Instantiate(enginePrefab, this.transform.position, Quaternion.identity);
                engineWeapon.transform.SetParent(weaponSocket.transform, false);
                _inventory.Add(WeaponID.Engine, engineWeapon.GetComponent<EngineWeapon>());
                _requestedWeapon = WeaponID.Engine;
                break;
        }
    }


    // If we have an equipped weapon, update it with the input the player passes to the weapon manager
    public void UpdateWeapons(float deltaTime, WeaponManagerInput input)
    {
        //Process player Input 
        ProcessInput(input);

        // First process any existing equip requests 
        ProcessEquipRequests();
        
        if (_equippedWeapon != null)
        {
            _equippedWeapon.UpdateWeapon(deltaTime, input);
        }
    }

    private void ProcessInput(WeaponManagerInput input)
    {
        // Take the players requested weapon input FIRST (as this is input for a specific weapon - probably to remove any weapons)
        // THEN if there is no weapon request allow swap weapon requests to go through
        _requestedWeaponSwap = input.SwapWeapon;
        _playerRequestedWeapon = input.PlayerRequestedWeapon;

        _requestedActionPressed = input.ActionPressed;
        _requestedActionHeld = input.ActionHeld;
        _requestedReadyPressed = input.ReadyPressed;
        _requestedReadyHeld = input.ReadyHeld;

        

        // SPECIFIC player requests for a weapon override existing weapon equip requests 
        if (_playerRequestedWeapon != WeaponID.NoChange)
        {
            _requestedWeapon = _playerRequestedWeapon;
        }
        // If the player has no specific weapon requests and just wants to swap, check that there isn't an existing request first then swap
        else if (_requestedWeapon == WeaponID.NoChange && _requestedWeaponSwap)
        {
            //if (_equippedWeapon != null)
           // {
                switch (_equippedWeaponID)
                {
                    case WeaponID.Engine:
                        if (_inventory.ContainsKey(WeaponID.FlareGun))
                        {
                            if (_inventory[WeaponID.FlareGun] != null) _requestedWeapon = WeaponID.FlareGun;
                        }
                        else
                        {
                            _requestedWeapon = WeaponID.None;
                        }
                            break;
                    case WeaponID.FlareGun:
                        if (_inventory.ContainsKey(WeaponID.Engine))
                        {
                            if (_inventory[WeaponID.Engine] != null) _requestedWeapon = WeaponID.Engine;
                        }
                        break;
                    case WeaponID.None:

                        if (_inventory.ContainsKey(WeaponID.Engine))
                        {
                            if (_inventory[WeaponID.Engine] != null) _requestedWeapon = WeaponID.Engine;
                        }
                        break;

                }
           // }
        }

    }


    private void ProcessEquipRequests()
    {
        // This way of flattening a bunch of if statements basically makes all the undesirable outcomes return
        // so we only reach the end and equip the weapon we've requested if all our checks have been met, like the equipped weapon being the same as the requested weapon

        if (_requestedWeapon == WeaponID.NoChange)
            return;

        if (_equippedWeapon == null)
        {
           if (_equippedWeaponID == WeaponID.None)
            {
                EquipWeapon(_requestedWeapon);
            }
            else
            {
               //throw new InvalidOperationException($"Weapon Manager is in an invalid state: _equippedWeapon is null but _equippedWeaponID is {_equippedWeaponID}.");
            }
                return;
        }

        if (_requestedWeapon == _equippedWeapon.ID)
        {
            _requestedWeapon = WeaponID.NoChange;
            return;
        }

        if (_equippedWeapon.IsBusy)
            return;

        if (_equippedWeapon.VisualState != WeaponVisualState.Unequipped)
        {
            _equippedWeapon.Unequip();
            return;
        }

        EquipWeapon(_requestedWeapon);
        _requestedWeapon = WeaponID.NoChange;
    }


    private void EquipWeapon(WeaponID newWeaponID)
    {
        // If we have a weapon to swap out, set it as the last equipped. If not, set last equipped to None. 
        if (_equippedWeaponID == WeaponID.None)
        {
            _lastEquippedID = WeaponID.None;
        }
        else
        {
            _lastEquippedID = _equippedWeapon.ID;
        }

        // Set the new equipped weapon to the weapon corresponding to the new weapon ID passed in the method
        // Call the 'Equip' method in this new equipped weapon 
        _equippedWeaponID = newWeaponID;

        if (_equippedWeaponID == WeaponID.None)
        {
            _equippedWeapon = null;
        }
        else
        {
            _equippedWeapon = _inventory[_equippedWeaponID];

            if (_equippedWeapon != null)
            {
                if (_equippedWeaponID != WeaponID.None)
                {
                    _equippedWeapon.Equip();
                }
            }
        }
    }

    // Get the weapon effects from whatever weapon is equipped
    public WeaponEffects GetCurrentEffects()
    {
        return _equippedWeapon != null
            ? _equippedWeapon.GetEffects()
            : default;
    }
}
