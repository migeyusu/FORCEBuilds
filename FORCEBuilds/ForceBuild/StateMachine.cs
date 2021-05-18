using System;
using System.Collections.Generic;

namespace FORCEBuild
{
    /// <summary>
    /// simple state machine，without trigger
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StateMachine<T> where T : struct
    {
        public event Action<Transaction> StateChanged;

        public State CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != null && value == _currentState)
                {
                    return;
                }

                var currentStateStatus = default(T);
                if (_currentState != null)
                {
                    currentStateStatus = _currentState.Status;
                }

                _currentState = value;
                OnStateChanged(new Transaction()
                {
                    From = currentStateStatus,
                    To = value.Status
                });
            }
        }

        private readonly Dictionary<T, State> _statesDictionary =
            new Dictionary<T, State>();

        private State _currentState;

        protected virtual void OnStateChanged(Transaction obj)
        {
            StateChanged?.Invoke(obj);
        }

        /// <summary>
        /// 为状态机添加或者取得已有的状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public State RegisterOrGetState(T status)
        {
            if (_statesDictionary.TryGetValue(status, out State value))
            {
                return value;
            }

            var state = new State() { Status = status };
            _statesDictionary.Add(status, state);
            return state;
        }

        public void MoveTo(T status, bool fireEvent = true)
        {
            MoveTo(status, null, fireEvent);
        }

        public void MoveTo(T status, object param, bool fireEvent = true)
        {
            if (!_statesDictionary.TryGetValue(status, out State state))
            {
                throw new NotSupportedException("Can't go to specific state.");
            }

            CurrentState?.OnExit(status); //发生异常后无法进入state
            if (fireEvent)
            {
                state.OnEnter(CurrentState?.Status, param);
            }
            CurrentState = state;
        }

        public class State
        {
            public T Status { get; set; }

            public event Action<Transaction> Enter;

            public event Action<Transaction> Exit;

            internal void OnEnter(T? previousStatus, object param)
            {
                var transaction = new Transaction()
                {
                    To = Status,
                    From = previousStatus,
                    Param = param
                };
                Enter?.Invoke(transaction);
            }

            internal void OnExit(T exitStatus)
            {
                var transaction = new Transaction()
                {
                    From = Status,
                    To = exitStatus,
                };
                Exit?.Invoke(transaction);
            }
        }

        public class Transaction
        {
            public T? From { get; set; }

            public T To { get; set; }

            public object Param { get; set; }
        }
    }
}