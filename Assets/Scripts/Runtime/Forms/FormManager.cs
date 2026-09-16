using UnityEngine;
using System.Collections.Generic;

public class FormManager : SingletonBase<FormManager>
{
    private int currentFormIndex;
    private float switchCooldown;
    public const int MAX_FORMS = 3;
    public const int PRIMARY_FORM_SLOT = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SwitchForm(int index)
    {
        var forms = GameManager.Instance.Player.ownedForms;
        if (index < 0 || index >= forms.Count || switchCooldown > 0f)
        {
            return;
        }

        currentFormIndex = index;
        switchCooldown = 3f;
        EventBus.Emit(EventTypes.FormSwitched, forms[index]);
    }

    public bool TryAddForm(string newFormId)
    {
        var player = GameManager.Instance.Player;
        
        if (player.ownedForms.Contains(newFormId))
        {
            return false;
        }

        if (player.ownedForms.Count < MAX_FORMS)
        {
            player.ownedForms.Add(newFormId);
            player.currentFormId = newFormId;
            EventBus.Emit(EventTypes.FormAdded, newFormId);

            return true;
        }
        else
        {
            EventBus.Emit(EventTypes.FormSlotFull, newFormId);

            return false;
        }
    }

    public bool CanReplaceSlot(int slotIndex)
    {
        if (slotIndex == PRIMARY_FORM_SLOT)
            return false;
        
        var player = GameManager.Instance.Player;
        return slotIndex > 0 && slotIndex < player.ownedForms.Count;
    }

    public bool ReplaceForm(int slotIndex, string newFormId)
    {
        var player = GameManager.Instance.Player;

        if (slotIndex == PRIMARY_FORM_SLOT)
        {

            return false;
        }

        if (slotIndex < 0 || slotIndex >= player.ownedForms.Count)
        {
            return false;
        }

        string oldFormId = player.ownedForms[slotIndex];
        
        player.ownedForms[slotIndex] = newFormId;
        player.currentFormId = newFormId;

        EventBus.Emit(EventTypes.FormReplaced, oldFormId);
        EventBus.Emit(EventTypes.FormSwitched, newFormId);
        

        return true;
    }

    public List<string> GetAvailableSlotsForReplacement()
    {
        var slots = new List<string>();
        var player = GameManager.Instance.Player;
        
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            if (i != PRIMARY_FORM_SLOT)
            {
                slots.Add(player.ownedForms[i]);
            }
        }
        
        return slots;
    }

    public int GetFormCount()
    {
        return GameManager.Instance.Player.ownedForms.Count;
    }

    public bool HasEmptySlot()
    {
        return GameManager.Instance.Player.ownedForms.Count < MAX_FORMS;
    }

    public string GetFormAtSlot(int index)
    {
        var forms = GameManager.Instance.Player.ownedForms;
        if (index >= 0 && index < forms.Count)
        {
            return forms[index];
        }
        return null;
    }

    void Update()
    {
        if (switchCooldown > 0f)
        {
            switchCooldown -= Time.deltaTime;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}