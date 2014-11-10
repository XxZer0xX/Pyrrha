

class ColorByName(object):
    (red,yellow,green,cyan,blue,magenta,white,dark_grey,light_grey) = (1,2,3,4,5,6,7,8,9)
    pass

def aci_is_valid(color):
    return color > 0 and color < 255
