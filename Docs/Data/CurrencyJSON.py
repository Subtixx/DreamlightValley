class MemoryReader:
    def __init__(self, byte_array):
        self.byte_array = byte_array
        self.index = 0

    def read(self, length: int):
        self.index += length
        return self.byte_array[self.index - length:self.index]

    def seek(self, offset: int, whence: int):
        if whence == 0:
            self.index = offset
        elif whence == 1:
            self.index += offset
        elif whence == 2:
            self.index = len(self.byte_array) + offset

    def read_string(self):
        str_length = int.from_bytes(self.read(1), byteorder='little', signed=False)
        str_bytes = self.read(str_length)
        str_read = str_bytes.decode('utf-8')
        eos = self.read(1) # Skip 1 byte (0x22 -> End of string?)
        if eos != b'"':
            raise Exception("Expected end of string, got: ", eos)
        return str_read

    def read_uint8(self):
        return int.from_bytes(self.read(1), byteorder='little', signed=False)

    def read_uint16(self):
        return int.from_bytes(self.read(2), byteorder='little', signed=False)

    def read_uint32(self):
        return int.from_bytes(self.read(4), byteorder='little', signed=False)

    def peek(self, length: int = 1):
        next_byte = self.read(length)
        self.seek(-length, 1)
        return next_byte

def read_chunk(chunk_data):
    # Read from chunk_data
    mem_reader = MemoryReader(chunk_data)
    # F1 1A 12
    # 12 BA             - 01 12 B7             01  0A 08
    # E9 F1 1A 12 84    - 01 08 84 EA 30 12 7E     0A 0C
    # E9 F1 1A 12 84    - 01 08 82 EA 30 12 7E     0A 0C
    # EA F1 1A 12 AE    - 01 08 A0 8D 06 12 A7 01  0A 03
    # E9 F1 1A 12 91    - 01 08 C0 9A 0C 12 8A 01  0A 05
    # F4 F1 1A 12 9D    - 01 08 E0 A7 12 12 96 01  0A 09
    # E9 F1 1A 12 88    - 01 08 85 EA 30 12 81 01  0A 0D
    if mem_reader.peek(2) == b'\x01\x12':
        mem_reader.read(5)
    else:
        mem_reader.read(7)
        if mem_reader.peek(1) == b'\x01':
            mem_reader.read(2)
        else:
            mem_reader.read(1)

    name = mem_reader.read_string()
    print("Name:", name)

if __name__ == "__main__":
    json_path = sys.argv[1]
    with open(json_path, "rb") as f:
        f.read(1) # ?! unknown
        while True:
            chunk_size = int.from_bytes(f.read(1), byteorder='little', signed=False)
            chunk_data = f.read(chunk_size)
    
            if chunk_data == b'':
                break
    
            read_chunk(chunk_data)
    
            f.read(2) # 1A 12
