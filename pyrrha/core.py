from Autodesk.AutoCAD.ApplicationServices import Application as _app

class PyrrhaManager(object):
    """
    Base Pyrrha Object 
    """
    def __init__(self):
        self.__document = _app.DocumentManager.MdiActiveDocument
        self.__transaction_manager = self.__document.TransactionManager
        pass

    @property
    def application(self):
        return _app

    @property
    def trans_manager(self):
        return self.__transaction_manager

    @property
    def active_document(self): 
        return self.__document

    @active_document.setter
    def active_document(self, value):
        self.__document = value
        _app.DocumentManager.MdiActiveDocument = value

    def write(self, message):
        self.__document.Editor.WriteMessage(message)
        pass

    def create_transaction(self):
        return self._transaction_manager.StartOpenCloseTransaction()

    pass
   

class CommandFlags(object):
    (TempShowDynDimension, Modal, Transparent, UsePickSet, Redraw, NoPerspective) = (-2147483648,0,1,2,4,8)
    (NoMultiplem, NoTileMode, NoPaperSpace, NoOem, Undefined, InProgess, Defun, NoNewStack) = (16,32,64,256,512,1024,2048,65536)
    (NoInternalLock, DockReadLock, DockExclusiveLock, Session, Interruptible) = (131072, 524286, 1048576, 2097152, 4194304)
    (NoHistory, NoUndoMarker, NoBlockEditor, NoActionRecording, ActionMacro, NoInferConstraint) = (8388608,16777216,33554432,67108864,134217728,1073741824)
    pass

class OpenMode(object):
     (ForRead, ForWrite, ForNotify) = (0,1,2)