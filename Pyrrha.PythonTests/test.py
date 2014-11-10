from pyrrha.decorators import CommandMethod, commit
from pyrrha.core import PyrrhaManager
from pyrrha.layers import LayerManager


@CommandMethod('layt')
def test():
    doc_manager = PyrrhaManager()
    doc = doc_manager.active_document
    manager = LayerManager()
    name = 'mikes Layer'
    manager.create_layer(Name=name, Color=3)
