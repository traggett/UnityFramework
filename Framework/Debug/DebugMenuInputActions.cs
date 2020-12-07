// GENERATED AUTOMATICALLY FROM 'Assets/ThirdParty/UnityFramework/Framework/Debug/DebugMenuInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Framework.Debug
{
    public class @DebugMenuInputActions : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @DebugMenuInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""DebugMenuInputActions"",
    ""maps"": [
        {
            ""name"": ""Menu"",
            ""id"": ""ddeb9f09-1011-49a6-a22d-57daaef06fca"",
            ""actions"": [
                {
                    ""name"": ""ToggleMenu"",
                    ""type"": ""Button"",
                    ""id"": ""0f95cdaa-9416-4332-b2e9-e6e482c2997a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Enter"",
                    ""type"": ""Button"",
                    ""id"": ""a4c0e81a-e5e9-49d2-9a04-8cd94daedd7b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Up"",
                    ""type"": ""Button"",
                    ""id"": ""b6ee39b6-f71b-4f7b-8cfc-ead7d08e4414"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Down"",
                    ""type"": ""Button"",
                    ""id"": ""ec76a6a7-bd6c-4608-8c5a-23559ed418de"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Left"",
                    ""type"": ""Button"",
                    ""id"": ""5cb5d26a-16ed-42d3-b7cb-c4b990c08f43"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Right"",
                    ""type"": ""Button"",
                    ""id"": ""034438e6-74cd-4406-bc01-8e1e8ece4d66"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ebb494a6-ddaf-44f8-907b-bb71d45d5ae4"",
                    ""path"": ""<Keyboard>/f3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0efb0e44-b2fe-4a3b-a36c-50dcbd63597f"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Enter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""54ac3a58-3388-460b-95fc-30aa47b32a78"",
                    ""path"": ""<Keyboard>/numpadEnter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Enter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dfa3951c-d0b2-45a2-858d-d929b7e5c480"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""895d76b0-e40d-46d7-bf00-80e3e1193f5b"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cc318cfe-65ca-4ba3-a6db-21f97fd04b58"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""70e0c11a-0b3b-4896-ac47-5eec25c3a3c3"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Menu
            m_Menu = asset.FindActionMap("Menu", throwIfNotFound: true);
            m_Menu_ToggleMenu = m_Menu.FindAction("ToggleMenu", throwIfNotFound: true);
            m_Menu_Enter = m_Menu.FindAction("Enter", throwIfNotFound: true);
            m_Menu_Up = m_Menu.FindAction("Up", throwIfNotFound: true);
            m_Menu_Down = m_Menu.FindAction("Down", throwIfNotFound: true);
            m_Menu_Left = m_Menu.FindAction("Left", throwIfNotFound: true);
            m_Menu_Right = m_Menu.FindAction("Right", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // Menu
        private readonly InputActionMap m_Menu;
        private IMenuActions m_MenuActionsCallbackInterface;
        private readonly InputAction m_Menu_ToggleMenu;
        private readonly InputAction m_Menu_Enter;
        private readonly InputAction m_Menu_Up;
        private readonly InputAction m_Menu_Down;
        private readonly InputAction m_Menu_Left;
        private readonly InputAction m_Menu_Right;
        public struct MenuActions
        {
            private @DebugMenuInputActions m_Wrapper;
            public MenuActions(@DebugMenuInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @ToggleMenu => m_Wrapper.m_Menu_ToggleMenu;
            public InputAction @Enter => m_Wrapper.m_Menu_Enter;
            public InputAction @Up => m_Wrapper.m_Menu_Up;
            public InputAction @Down => m_Wrapper.m_Menu_Down;
            public InputAction @Left => m_Wrapper.m_Menu_Left;
            public InputAction @Right => m_Wrapper.m_Menu_Right;
            public InputActionMap Get() { return m_Wrapper.m_Menu; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MenuActions set) { return set.Get(); }
            public void SetCallbacks(IMenuActions instance)
            {
                if (m_Wrapper.m_MenuActionsCallbackInterface != null)
                {
                    @ToggleMenu.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnToggleMenu;
                    @ToggleMenu.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnToggleMenu;
                    @ToggleMenu.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnToggleMenu;
                    @Enter.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnEnter;
                    @Enter.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnEnter;
                    @Enter.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnEnter;
                    @Up.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnUp;
                    @Up.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnUp;
                    @Up.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnUp;
                    @Down.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnDown;
                    @Down.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnDown;
                    @Down.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnDown;
                    @Left.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnLeft;
                    @Left.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnLeft;
                    @Left.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnLeft;
                    @Right.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnRight;
                    @Right.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnRight;
                    @Right.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnRight;
                }
                m_Wrapper.m_MenuActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @ToggleMenu.started += instance.OnToggleMenu;
                    @ToggleMenu.performed += instance.OnToggleMenu;
                    @ToggleMenu.canceled += instance.OnToggleMenu;
                    @Enter.started += instance.OnEnter;
                    @Enter.performed += instance.OnEnter;
                    @Enter.canceled += instance.OnEnter;
                    @Up.started += instance.OnUp;
                    @Up.performed += instance.OnUp;
                    @Up.canceled += instance.OnUp;
                    @Down.started += instance.OnDown;
                    @Down.performed += instance.OnDown;
                    @Down.canceled += instance.OnDown;
                    @Left.started += instance.OnLeft;
                    @Left.performed += instance.OnLeft;
                    @Left.canceled += instance.OnLeft;
                    @Right.started += instance.OnRight;
                    @Right.performed += instance.OnRight;
                    @Right.canceled += instance.OnRight;
                }
            }
        }
        public MenuActions @Menu => new MenuActions(this);
        public interface IMenuActions
        {
            void OnToggleMenu(InputAction.CallbackContext context);
            void OnEnter(InputAction.CallbackContext context);
            void OnUp(InputAction.CallbackContext context);
            void OnDown(InputAction.CallbackContext context);
            void OnLeft(InputAction.CallbackContext context);
            void OnRight(InputAction.CallbackContext context);
        }
    }
}
