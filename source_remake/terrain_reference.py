"""Recovered terrain predicates; ranges are inclusive/exclusive, not design guesses.
Original RISING-ORIG.EXE SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.
These predicates do not implement movement, terrain costs, or an entire game.
"""
PREDICATES = {
 'TerrainEstMarecage': (0x476239, []),
 'TerrainEstSable': (0x47623c, [(0x11d,0x15d)]),
 'TerrainEstFrontiereHerbe': (0x47626b, [(0x10,0x1f)]),
 'TerrainEstHerbe': (0x476296, [(0x15d,0x1a5)]),
 'TerrainEstFrontiereCailloux': (0x4762ec, [(0x2e,0x3d)]),
 'TerrainEstCailloux': (0x476317, [(0xdd,0x11d)]),
 'TerrainEstFrontiereArride': (0x476346, [(0x1f,0x2e)]),
 'TerrainEstArride': (0x476371, [(0x9d,0xdd),(0x84,0x87)]),
 'TerrainEstFrontiereEau': (0x4763c7, [(0x4e,0x5e)]),
 'TerrainEstEau': (0x4763f2, [(0x94,0x9d)]),
 'TerrainEstFrontiereCharbon': (0x476421, [(0x6e,0x7d)]),
 'TerrainEstCharbon': (0x47644c, [(0x7e,0x7f)]),
 'TerrainEstMontagne': (0x476468, [(0x1dd,0x260)]),
 'TerrainEstHautMontagne': (0x476497, [(0x257,0x261)]),
 'TerrainEstFrontiereMagma': (0x4764c6, [(0x5e,0x6d)]),
 'TerrainEstMagma': (0x4764f1, [(0x8b,0x93)]),
}

def classify(name, tile_id):
    if not 0 <= tile_id <= 65535: raise ValueError('tile_id must be uint16')
    return any(lo <= tile_id < hi for lo,hi in PREDICATES[name][1])
