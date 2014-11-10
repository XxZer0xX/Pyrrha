from Autodesk.AutoCAD.Internal import Utils, CommandCallback
from sys import path
# add path to functools
path.append(r'C:\Program Files (x86)\IronPython 2.7\Lib')
from core import CommandFlags

def register_command(group,name,flags,func):
    Utils.AddCommand(group,name,name,flags,CommandCallback(func))

def CommandMethod(commandName,commandFlags = CommandFlags.Modal):
    """
    Decorator for registering a command with AutoCAD
    
    Usage:
        @CommandMethod("command_name", CommandFlags.Modal | CommandFlags.AnotherFlag)
        def command_method():
            ...
            ...  
    """
    def method_decorator(func):
        register_command('pyrrha_commands', commandName, commandFlags, func)
    return method_decorator

def LispFunction(commandName, commandFlags = CommandFlags.Modal | CommandFlags.Defun):
    """
    Decorator for registering a command that can be invoked with AutoLISP
    
    Usage:
        @LispFunction("command_name", CommandFlags.Modal | CommandFlags.Defun)
        def lisp_function():
            ...
            ...  
    """
    def method_decorator(func):
        register_command('pyrrha_lisp_commands',commandName,commandFlags,func)
    return method_decorator

def commit():
    """ 
    This Decorator commits and disposes of the transaction used in the method it decorates.
    
    Requirment: 
        You must return the transaction from the method.
        if you are returning values you must return the transaction
        as the first element in a list.

    Usage:
        def method_with_return_value(*args,**kwargs):
            ...
            ...
            return [transaction, value]
    """
    def method_decorator(func):
        result = func()

        value = None
        if result is list:
            trans = result[0]
            value = result[1]
        else:
            trans = result

        if trans is None:
            return

        trans.Commit()
        trans.Dispose()

        if value is not None:
            return value
    return method_decorator


