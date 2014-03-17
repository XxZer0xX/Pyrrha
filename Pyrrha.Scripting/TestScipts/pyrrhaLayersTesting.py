# Pyrrha testing Script

newlayer = createlayer('test1',3,'HIDDEN')
print('layer {0} color: {1}'.format(newlayer.Name,newlayer.Color.ColorNameForDisplay))
oldColor = newlayer.Color.ColorNameForDisplay
newlayer.Color = fromcolorindex(5)
oldlayername = newlayer.Name
newlayer.Name = 'difName'
print('layer "{0}" color has changed from {1} to {2}'.format(oldlayername,oldColor,newlayer.Color.ColorNameForDisplay))
print('layer "{0}" has changed to {1}'.format(oldlayername,newlayer.Name))
print('{0} was the distance specified'.format(getdistance('\nselect two points: ')))
print('{0} was the double specified'.format(getdouble('\nspecify double: ')))
print('{0} was the file name for open'.format(getfilenameforopen('\nselect file: ')))
print('{0} was the file name for save'.format(getfilenameforsave('\nselect file: ')))
print('{0} was the int specified'.format(getinteger('\nspecify int: ')))
print('{0} was the point specified'.format(getpoint('\nselect a point: ')))
print('{0} was the string specified'.format(getstring('\nspecify string: ',True)))
regen()
print('USERI1 = {0}'.format(getvar('useri1')))
setvar('useri1',78)
print('USERI1 = {0} '.format(getvar('useri1')))
alert('this is a testing alert')
confirmallchanges()