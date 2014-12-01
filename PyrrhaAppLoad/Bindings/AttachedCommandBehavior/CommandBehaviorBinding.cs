#region Referenceing

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

#endregion

namespace PyrrhaAppLoad.Bindings.AttachedCommandBehavior
{
    /// <summary>
    ///     Defines the command behavior binding
    /// </summary>
    public class CommandBehaviorBinding : IDisposable
    {
        #region Properties

        /// <summary>
        ///     Get the owner of the CommandBinding ex: a Button
        ///     This property can only be set from the BindEvent Method
        /// </summary>
        public DependencyObject Owner { get; private set; }

        /// <summary>
        ///     The event name to hook up to
        ///     This property can only be set from the BindEvent Method
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        ///     The event info of the event
        /// </summary>
        public EventInfo Event { get; private set; }

        /// <summary>
        ///     Gets the EventHandler for the binding with the event
        /// </summary>
        public Delegate EventHandler { get; private set; }

        #region Execution

        //stores the strategy of how to execute the event handler
        Action<object> action;
        ICommand command;
        IExecutionStrategy strategy;

        /// <summary>
        ///     Gets or sets a CommandParameter
        /// </summary>
        public object CommandParameter { get; set; }

        /// <summary>
        ///     The command to execute when the specified event is raised
        /// </summary>
        public ICommand Command
        {
            get { return command; }
            set
            {
                command = value;
                //set the execution strategy to execute the command
                strategy = new CommandExecutionStrategy {Behavior = this};
            }
        }

        /// <summary>
        ///     Gets or sets the Action
        /// </summary>
        public Action<object> Action
        {
            get { return action; }
            set
            {
                action = value;
                // set the execution strategy to execute the action
                strategy = new ActionExecutionStrategy {Behavior = this};
            }
        }

        #endregion

        #endregion

        //Creates an EventHandler on runtime and registers that handler to the Event specified

        bool disposed;

        #region IDisposable Members

        /// <summary>
        ///     Unregisters the EventHandler from the Event
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Event.RemoveEventHandler(Owner, EventHandler);
                disposed = true;
            }
        }

        #endregion

        public void BindEvent(DependencyObject owner, string eventName)
        {
            EventName = eventName;
            Owner = owner;
            Event = Owner.GetType().GetEvent(EventName, BindingFlags.Public | BindingFlags.Instance);
            if (Event == null)
                throw new InvalidOperationException(String.Format("Could not resolve event name {0}", EventName));

            //Create an event handler for the event that will call the ExecuteCommand method
            EventHandler = EventHandlerGenerator.CreateDelegate(
                Event.EventHandlerType,
                typeof (CommandBehaviorBinding).GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance), this);
            //Register the handler to the Event
            Event.AddEventHandler(Owner, EventHandler);
        }

        /// <summary>
        ///     Executes the strategy
        /// </summary>
        public void Execute()
        {
            strategy.Execute(CommandParameter);
        }
    }
}