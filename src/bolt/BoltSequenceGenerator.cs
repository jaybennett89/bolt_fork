struct BoltSequenceGenerator {
    uint mask;
    uint sequence;

    public BoltSequenceGenerator (int bits)
        : this(bits, 0u) {
    }

    public BoltSequenceGenerator (int bits, uint start) {
        mask = (1u << bits) - 1u;
        sequence = start & mask;
    }

    public uint Next () {
        sequence += 1u;
        sequence &= mask;
        return sequence;
    }
}
