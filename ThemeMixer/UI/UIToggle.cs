﻿using System;
using System.IO;
using ColossalFramework.UI;
using ICities;
using ThemeMixer.Resources;
using ThemeMixer.Serialization;
using UnityEngine;
using UnifiedUI.Helpers;

namespace ThemeMixer.UI
{
    public class UIToggle : UIButton
    {
        public delegate void UIToggleClickedEventHandler();
        public event UIToggleClickedEventHandler EventUIToggleClicked;
        public KeyCode _hotkey = KeyCode.T;
        public static string referenceHotkey;

        private bool _toggled;
        private Vector3 DeltaPos { get; set; }

        public override void Start()
        {
            base.Start();
            name = "Theme Mixer Toggle";
            atlas = UISprites.Atlas;
            normalBgSprite = UISprites.UIToggleIcon;
            hoveredBgSprite = UISprites.UIToggleIconHovered;
            pressedBgSprite = UISprites.UIToggleIconPressed;
            absolutePosition = SerializationService.Instance.GetUITogglePosition() ?? GetDefaultPosition();

            // Read the hotkey from the TM2.5_key_config.txt file
            string hotkeyFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TM2.5_key_config.txt"
            );

            if (!File.Exists(hotkeyFilePath))
            {
                // Create the file with a default hotkey if it doesn't exist
                KeyCode defaultHotkey = KeyCode.None; // Replace with your desired default hotkey
                UIToggle.referenceHotkey = defaultHotkey.ToString();
                File.WriteAllText(hotkeyFilePath, defaultHotkey.ToString());
            }

            string hotkeyString = File.ReadAllText(hotkeyFilePath).Trim();
            KeyCode hotkey;
            if (Enum.IsDefined(typeof(KeyCode), hotkeyString))
            {
                hotkey = (KeyCode)Enum.Parse(typeof(KeyCode), hotkeyString);

                // Check if the hotkey has changed
                if (hotkey != _hotkey)
                {
                    // Save the new hotkey
                    _hotkey = hotkey;

                    // Perform any necessary actions with the new hotkey
                    // ...

                    // Show a message indicating that the hotkey has been updated
                    Debug.Log("New hotkey saved: " + _hotkey);
                    

                }
            }
            else
            {
                Debug.LogError("Invalid hotkey: " + hotkeyString);
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Hotkey Save Error", "Failed to save the hotkey settings.", true);
            }


            // Add UUI button.
            var uuiButton = UUIHelpers.RegisterCustomButton(
                name: (Mod.Instance as Mod)?.Name,
                groupName: null, // default group
                tooltip: (Mod.Instance as Mod)?.Name,
                icon: UISprites.Atlas.GetSpriteTexture("UIToggleIcon"),
                onToggle: (value) => SimulateClick()
                );
        }

        public class LoadingExtension : LoadingExtensionBase
        {
            public override void OnLevelLoaded(LoadMode mode)
            {
                if (mode == LoadMode.NewGame)
                {
                    // Create a canvas object to hold the UI elements
                    GameObject canvasObject = new GameObject("Canvas");
                    Canvas canvas = canvasObject.AddComponent<Canvas>();

                    // Add a ColossalFramework UI component to display the message panel
                    UIPanel uIPanel = canvasObject.AddComponent<UIPanel>();

                    // Set the position and size of the panel
                    uIPanel.relativePosition = new Vector3((Screen.width - 400) / 2f, (Screen.height - 200) / 2f);
                    uIPanel.width = 400;
                    uIPanel.height = 200;

                    // Add a label for the welcome message
                    UILabel welcomeLabel = uIPanel.AddUIComponent<UILabel>();

                    // Set the position and size of the welcome label
                    welcomeLabel.relativePosition = new Vector3(10, 10);
                    welcomeLabel.width = uIPanel.width - 20;
                    welcomeLabel.height = 30;

                    // Set the text for the welcome label
                    welcomeLabel.text = "Welcome to Theme Mixer 2.5!";

                    // Add a label for additional text
                    UILabel additionalLabel = uIPanel.AddUIComponent<UILabel>();

                    // Set the position and size of the additional label
                    additionalLabel.relativePosition = new Vector3(10, 50);
                    additionalLabel.width = uIPanel.width - 20;
                    additionalLabel.height = 30;

                    // Set the text for the additional label
                    additionalLabel.text = "Thanks for subscribing to Theme Mixer 2.5. For support head to the Steam Workshop page.";

                    // Add a ColossalFramework UI component for the close button
                    UIButton closeButton = uIPanel.AddUIComponent<UIButton>();

                    // Set the position and size of the close button
                    closeButton.relativePosition = new Vector3(uIPanel.width - 40, 10);
                    closeButton.width = 30;
                    closeButton.height = 30;

                    // Set the close button's text and click event
                    closeButton.text = "X";
                    closeButton.eventClick += (component, eventParam) =>
                    {
                        // Destroy the panel and canvas when the close button is clicked
                        GameObject.Destroy(canvasObject);
                    };
                }
            }
        }





        public Vector2 GetDefaultPosition()
        {
            UIComponent referenceComponent = GetUIView().FindUIComponent<UIComponent>("UnlockButton");
            Vector2 pos = new Vector2(referenceComponent.absolutePosition.x + 80.0f, referenceComponent.absolutePosition.y + (referenceComponent.height - height) / 2);
            return pos;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (!p.buttons.IsFlagSet(UIMouseButton.Left)) return;
            _toggled = !_toggled;
            EventUIToggleClicked?.Invoke();
            normalBgSprite = _toggled ? UISprites.UIToggleIconFocused : UISprites.UIToggleIcon;
        }

        public override void Update()
        {
            base.Update();

            // Handle the key press event using the current hotkey value
            if (Input.GetKeyDown(_hotkey))
            {
                Debug.Log("Hotkey pressed: " + _hotkey);
                EventUIToggleClicked?.Invoke();
                _toggled = !_toggled;
                normalBgSprite = _toggled ? UISprites.UIToggleIconFocused : UISprites.UIToggleIcon;
            }
        }


        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;

                DeltaPos = absolutePosition - mousePos;
                BringToFront();
            }
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = m_OwnerView.fixedHeight - mousePos.y;
                absolutePosition = mousePos + DeltaPos;
                SerializationService.Instance.SetUITogglePosition(new Vector2(absolutePosition.x, absolutePosition.y));
            }
        }

        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            base.OnMouseUp(p);
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                SerializationService.Instance.SaveData();
            }
        }
    }
}
