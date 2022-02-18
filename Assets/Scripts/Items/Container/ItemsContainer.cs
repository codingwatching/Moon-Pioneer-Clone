﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Items.Container
{
    // See the bottom of code. There is an interesting bug on [Line ~70]
    // https://github.com/microsoft/referencesource/issues/164

    public class ItemsContainer : MonoBehaviour
    {
        public bool IsFull
        {
            get
            {
                return _items.Count >= _capacity;
            }
        }
        public int Count
        {
            get
            {
                return _items.Count;
            }
        }
        public int Capacity { get => _capacity; }

        [SerializeField, Tooltip("Don't change due runtime!")] private int _capacity = 10;
        [SerializeField] private int _sizeX = 3;
        [SerializeField] private int _sizeZ = 3;

        [Inject] private ContainerSlotsFactory _slotsFactory;
        private List<Item> _items = new List<Item>();
        private ContainerSlot[] _slots;                                 // Markdown #2

        private void Start()
        {
            _slots = _slotsFactory.CreateSlots(transform, _capacity, _sizeX, _sizeZ);
        }

        public bool AddItem(Item item)
        {
            if (!CanAddItem())
            {
                Debug.Log("Cant add item");
                return false;
            }

            _items.Add(item);
            item.GoToSlot(this, null);

            return true;
        }
        public bool TakeItem(out Item item)
        {
            if (!CanTakeItem())
            {
                item = null;
                return false;
            }

            Item lastItem = _items[_items.Count - 1];
            item = lastItem;

            IEnumerable<ContainerSlot> busySlotsWithItem = _slots.Where(x => x.BusyItem == lastItem);
            if (busySlotsWithItem.Count() != 0)
            {
                busySlotsWithItem.First().Detach();
            }

            _items.RemoveAt(_items.Count - 1);

            return true;
        }

        public bool CanAddItem()
        {
            return !IsFull;
        }
        public bool CanTakeItem()
        {
            return _items.Count > 0;
        }

        public ContainerSlot AttachItemToSlot(Item item)
        {
            IEnumerable<ContainerSlot> availableSlots = _slots.Where(x => x.IsBusy == false);       // Markdown #1
            ContainerSlot availableSlot = availableSlots.FirstOrDefault();
            availableSlot.Attach(item);
            return availableSlot;

            // Somehow, System.Linq thread can be crashed without any exceptions [Markdown #1]
            // In my case in debug mode it shows: "The thread 0x48e32be0 has exited with code 0 (0x0)"
            // I found out that can crash in async-await method if array will be null. Like in my case, 
            // I found bug in my code, that _slots [Markdown #2] was null. I have no idea how can I try-catch this
            // exception, except checking _slots for != null.
        }
        public void DetachItemFromSlot(Item item)
        {
            ContainerSlot itemSlot = _slots.Where(x => x.BusyItem == item).First();
            itemSlot.Detach();
        }
    }
}
