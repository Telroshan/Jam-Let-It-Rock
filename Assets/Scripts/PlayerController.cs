﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Move _selectedMove = Move.None;
    public int score;

    public int playerId;
    private bool Ready => playerId > 0;

    private PlayerController _otherPlayer;
    private PlayerInput _playerInput;
    private GameMode _gameMode = GameMode.Menu;

    public SmashMinigame SmashMinigame { get; set; }

    public enum GameMode
    {
        Menu,
        Preparation,
        InGame,
        SmashMinigame,
        DontPressMinigame,
    }

    public bool IsPrepared { get; private set; }

    public Action<PlayerController> onJoined;
    public Action<PlayerController> onPrepared;
    public Action<PlayerController> onDisconnected;
    public Action<PlayerController, Move> onMoveSelect;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _playerInput = GetComponent<PlayerInput>();
        _otherPlayer = FindObjectsOfType<PlayerController>().First(x => x != this);
    }

    public void SetGameMode(GameMode mode)
    {
        if (_gameMode == GameMode.Menu && mode == GameMode.Preparation)
        {
            Debug.Log("Switched to game map");
            _playerInput.SwitchCurrentActionMap("Game");
            _playerInput.uiInputModule = null;
        }

        _gameMode = mode;
    }

    public Move GetMove()
    {
        if (_selectedMove != Move.None) return _selectedMove;
        return (Move) Random.Range((int) Move.Rock, (int) Move.Scissors + 1);
    }

    private void SetMove(Move move)
    {
        _selectedMove = move;
        onMoveSelect?.Invoke(this, _selectedMove);
    }

    public void BeginTurn()
    {
        SetMove(Move.None);
        Debug.Log("Begin turn");
        SetGameMode(GameMode.InGame);
    }

    public void Rock(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        switch (_gameMode)
        {
            case GameMode.Preparation:
            case GameMode.InGame:
                Debug.Log("ROCK");
                SetMove(Move.Rock);
                break;
            case GameMode.SmashMinigame:
                SmashMinigame.getPressedInput(playerId, Move.Rock);
                break;
        }
    }

    public void Paper(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        switch (_gameMode)
        {
            case GameMode.Preparation:
            case GameMode.InGame:
                Debug.Log("PAPER");
                SetMove(Move.Paper);
                break;
            case GameMode.SmashMinigame:
                SmashMinigame.getPressedInput(playerId, Move.Paper);
                break;
        }
    }

    public void Scissors(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        switch (_gameMode)
        {
            case GameMode.Preparation:
            case GameMode.InGame:
                Debug.Log("SCISSORS");
                SetMove(Move.Scissors);
                break;
            case GameMode.SmashMinigame:
                SmashMinigame.getPressedInput(playerId, Move.Scissors);
                break;
        }
    }

    public void Join(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        switch (_gameMode)
        {
            case GameMode.Menu:
            {
                if (Ready) return;
                Debug.Log("JOIN");
                playerId = !_otherPlayer.Ready || _otherPlayer.playerId == 2 ? 1 : 2;
                if (playerId == 1 && !_playerInput.uiInputModule)
                {
                    _playerInput.uiInputModule = _otherPlayer._playerInput.uiInputModule
                        ? _otherPlayer._playerInput.uiInputModule
                        : FindObjectOfType<InputSystemUIInputModule>();
                    _otherPlayer._playerInput.uiInputModule = null;
                    _playerInput.uiInputModule.actionsAsset = _playerInput.actions;
                    _playerInput.uiInputModule.UpdateModule();
                }

                onJoined?.Invoke(this);
                break;
            }
        }
    }

    public void Prepared(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        IsPrepared = true;
        Debug.Log("READY");
        onPrepared?.Invoke(this);
    }


    public void Disconnected()
    {
        onDisconnected?.Invoke(this);
        if (_gameMode == GameMode.Menu)
        {
            playerId = 0;
        }
    }
}