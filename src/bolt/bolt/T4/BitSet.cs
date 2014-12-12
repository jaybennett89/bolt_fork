

using System;

namespace Bolt {

	public class BitSet : BoltObject {
		public const int BITSET_LONGS = 16;
		internal static readonly BitSet Full;

		static BitSet() {
		  Full = new BitSet();
		  
						Full.Bits0 |= uint.MaxValue; 
						Full.Bits1 |= uint.MaxValue; 
						Full.Bits2 |= uint.MaxValue; 
						Full.Bits3 |= uint.MaxValue; 
						Full.Bits4 |= uint.MaxValue; 
						Full.Bits5 |= uint.MaxValue; 
						Full.Bits6 |= uint.MaxValue; 
						Full.Bits7 |= uint.MaxValue; 
						Full.Bits8 |= uint.MaxValue; 
						Full.Bits9 |= uint.MaxValue; 
						Full.Bits10 |= uint.MaxValue; 
						Full.Bits11 |= uint.MaxValue; 
						Full.Bits12 |= uint.MaxValue; 
						Full.Bits13 |= uint.MaxValue; 
						Full.Bits14 |= uint.MaxValue; 
						Full.Bits15 |= uint.MaxValue; 
					}


					ulong Bits0;
					ulong Bits1;
					ulong Bits2;
					ulong Bits3;
					ulong Bits4;
					ulong Bits5;
					ulong Bits6;
					ulong Bits7;
					ulong Bits8;
					ulong Bits9;
					ulong Bits10;
					ulong Bits11;
					ulong Bits12;
					ulong Bits13;
					ulong Bits14;
					ulong Bits15;
		
		public BitSet(
		
						
				ulong bits0

				 , 
						
				ulong bits1

				 , 
						
				ulong bits2

				 , 
						
				ulong bits3

				 , 
						
				ulong bits4

				 , 
						
				ulong bits5

				 , 
						
				ulong bits6

				 , 
						
				ulong bits7

				 , 
						
				ulong bits8

				 , 
						
				ulong bits9

				 , 
						
				ulong bits10

				 , 
						
				ulong bits11

				 , 
						
				ulong bits12

				 , 
						
				ulong bits13

				 , 
						
				ulong bits14

				 , 
						
				ulong bits15

				
					) {
							this.Bits0 = bits0;
							this.Bits1 = bits1;
							this.Bits2 = bits2;
							this.Bits3 = bits3;
							this.Bits4 = bits4;
							this.Bits5 = bits5;
							this.Bits6 = bits6;
							this.Bits7 = bits7;
							this.Bits8 = bits8;
							this.Bits9 = bits9;
							this.Bits10 = bits10;
							this.Bits11 = bits11;
							this.Bits12 = bits12;
							this.Bits13 = bits13;
							this.Bits14 = bits14;
							this.Bits15 = bits15;
					}
		
		

		public BitSet() {
		}

		public bool IsZero {
		  get {
			return
								(this.Bits0 == 0UL) &&
								(this.Bits1 == 0UL) &&
								(this.Bits2 == 0UL) &&
								(this.Bits3 == 0UL) &&
								(this.Bits4 == 0UL) &&
								(this.Bits5 == 0UL) &&
								(this.Bits6 == 0UL) &&
								(this.Bits7 == 0UL) &&
								(this.Bits8 == 0UL) &&
								(this.Bits9 == 0UL) &&
								(this.Bits10 == 0UL) &&
								(this.Bits11 == 0UL) &&
								(this.Bits12 == 0UL) &&
								(this.Bits13 == 0UL) &&
								(this.Bits14 == 0UL) &&
								(this.Bits15 == 0UL) &&
								true;
		  }
		}

		public void Set(int bit) {
		  switch (bit / 64) {
			
						case 0: this.Bits0 |= (1UL << (bit % 64)); break;
						case 1: this.Bits1 |= (1UL << (bit % 64)); break;
						case 2: this.Bits2 |= (1UL << (bit % 64)); break;
						case 3: this.Bits3 |= (1UL << (bit % 64)); break;
						case 4: this.Bits4 |= (1UL << (bit % 64)); break;
						case 5: this.Bits5 |= (1UL << (bit % 64)); break;
						case 6: this.Bits6 |= (1UL << (bit % 64)); break;
						case 7: this.Bits7 |= (1UL << (bit % 64)); break;
						case 8: this.Bits8 |= (1UL << (bit % 64)); break;
						case 9: this.Bits9 |= (1UL << (bit % 64)); break;
						case 10: this.Bits10 |= (1UL << (bit % 64)); break;
						case 11: this.Bits11 |= (1UL << (bit % 64)); break;
						case 12: this.Bits12 |= (1UL << (bit % 64)); break;
						case 13: this.Bits13 |= (1UL << (bit % 64)); break;
						case 14: this.Bits14 |= (1UL << (bit % 64)); break;
						case 15: this.Bits15 |= (1UL << (bit % 64)); break;
			
			default:
			  throw new IndexOutOfRangeException();
		  }

		  Assert.False(IsZero);
		}

		public void Clear(int bit) {
		  switch (bit / 64) {

						case 0: this.Bits0 &= ~(1UL << (bit % 64)); break;
						case 1: this.Bits1 &= ~(1UL << (bit % 64)); break;
						case 2: this.Bits2 &= ~(1UL << (bit % 64)); break;
						case 3: this.Bits3 &= ~(1UL << (bit % 64)); break;
						case 4: this.Bits4 &= ~(1UL << (bit % 64)); break;
						case 5: this.Bits5 &= ~(1UL << (bit % 64)); break;
						case 6: this.Bits6 &= ~(1UL << (bit % 64)); break;
						case 7: this.Bits7 &= ~(1UL << (bit % 64)); break;
						case 8: this.Bits8 &= ~(1UL << (bit % 64)); break;
						case 9: this.Bits9 &= ~(1UL << (bit % 64)); break;
						case 10: this.Bits10 &= ~(1UL << (bit % 64)); break;
						case 11: this.Bits11 &= ~(1UL << (bit % 64)); break;
						case 12: this.Bits12 &= ~(1UL << (bit % 64)); break;
						case 13: this.Bits13 &= ~(1UL << (bit % 64)); break;
						case 14: this.Bits14 &= ~(1UL << (bit % 64)); break;
						case 15: this.Bits15 &= ~(1UL << (bit % 64)); break;
			
			default:
			  throw new IndexOutOfRangeException();
		  }
		}


		public void Combine(BitSet other) {
						this.Bits0 |= other.Bits0;
						this.Bits1 |= other.Bits1;
						this.Bits2 |= other.Bits2;
						this.Bits3 |= other.Bits3;
						this.Bits4 |= other.Bits4;
						this.Bits5 |= other.Bits5;
						this.Bits6 |= other.Bits6;
						this.Bits7 |= other.Bits7;
						this.Bits8 |= other.Bits8;
						this.Bits9 |= other.Bits9;
						this.Bits10 |= other.Bits10;
						this.Bits11 |= other.Bits11;
						this.Bits12 |= other.Bits12;
						this.Bits13 |= other.Bits13;
						this.Bits14 |= other.Bits14;
						this.Bits15 |= other.Bits15;
					}

		public void ClearAll() {
						this.Bits0 = 0UL;
						this.Bits1 = 0UL;
						this.Bits2 = 0UL;
						this.Bits3 = 0UL;
						this.Bits4 = 0UL;
						this.Bits5 = 0UL;
						this.Bits6 = 0UL;
						this.Bits7 = 0UL;
						this.Bits8 = 0UL;
						this.Bits9 = 0UL;
						this.Bits10 = 0UL;
						this.Bits11 = 0UL;
						this.Bits12 = 0UL;
						this.Bits13 = 0UL;
						this.Bits14 = 0UL;
						this.Bits15 = 0UL;
					}
		
		public bool IsSet(int bit) {
		  ulong b = 1UL << (bit % 64); 

		  switch (bit / 64) {

						case 0: return (this.Bits0 & b) == b;
						case 1: return (this.Bits1 & b) == b;
						case 2: return (this.Bits2 & b) == b;
						case 3: return (this.Bits3 & b) == b;
						case 4: return (this.Bits4 & b) == b;
						case 5: return (this.Bits5 & b) == b;
						case 6: return (this.Bits6 & b) == b;
						case 7: return (this.Bits7 & b) == b;
						case 8: return (this.Bits8 & b) == b;
						case 9: return (this.Bits9 & b) == b;
						case 10: return (this.Bits10 & b) == b;
						case 11: return (this.Bits11 & b) == b;
						case 12: return (this.Bits12 & b) == b;
						case 13: return (this.Bits13 & b) == b;
						case 14: return (this.Bits14 & b) == b;
						case 15: return (this.Bits15 & b) == b;
			
			default:
			  throw new IndexOutOfRangeException();
		  }
		}
		
		public Iterator GetIterator() {
		  return new Iterator(this);
		}

		  public struct Iterator {
			int number;
			int numberBit;

			BitSet set;

			public Iterator(BitSet set) {
			  this.number = 0;
			  this.numberBit = 0;
			  this.set = set;
			}

			public bool Next(out int bit) {
			  ulong bits;

			  while (true) {

				switch (number) {

									case 0: bits = set.Bits0; break;
									case 1: bits = set.Bits1; break;
									case 2: bits = set.Bits2; break;
									case 3: bits = set.Bits3; break;
									case 4: bits = set.Bits4; break;
									case 5: bits = set.Bits5; break;
									case 6: bits = set.Bits6; break;
									case 7: bits = set.Bits7; break;
									case 8: bits = set.Bits8; break;
									case 9: bits = set.Bits9; break;
									case 10: bits = set.Bits10; break;
									case 11: bits = set.Bits11; break;
									case 12: bits = set.Bits12; break;
									case 13: bits = set.Bits13; break;
									case 14: bits = set.Bits14; break;
									case 15: bits = set.Bits15; break;
				
				  case 16:
					bit = -1;
					return false;

				  default:
					throw new InvalidOperationException();
				}

				if (bits == 0) {
				  number = number + 1;
				  numberBit = 0;
				}
				else {
				  for (; numberBit < 64; ++numberBit) {
					if ((bits & (1UL << numberBit)) != 0UL) {
					  switch (number) {
													case 0: set.Bits0 &= ~(1UL << numberBit); break;
													case 1: set.Bits1 &= ~(1UL << numberBit); break;
													case 2: set.Bits2 &= ~(1UL << numberBit); break;
													case 3: set.Bits3 &= ~(1UL << numberBit); break;
													case 4: set.Bits4 &= ~(1UL << numberBit); break;
													case 5: set.Bits5 &= ~(1UL << numberBit); break;
													case 6: set.Bits6 &= ~(1UL << numberBit); break;
													case 7: set.Bits7 &= ~(1UL << numberBit); break;
													case 8: set.Bits8 &= ~(1UL << numberBit); break;
													case 9: set.Bits9 &= ~(1UL << numberBit); break;
													case 10: set.Bits10 &= ~(1UL << numberBit); break;
													case 11: set.Bits11 &= ~(1UL << numberBit); break;
													case 12: set.Bits12 &= ~(1UL << numberBit); break;
													case 13: set.Bits13 &= ~(1UL << numberBit); break;
													case 14: set.Bits14 &= ~(1UL << numberBit); break;
													case 15: set.Bits15 &= ~(1UL << numberBit); break;
												
						default:
						throw new InvalidOperationException();
					  }

					  // set bit we found
					  bit = (number * 64) + numberBit;

					  // done!
					  return true;
					}
				  }

				  throw new InvalidOperationException();
				}
			  }
			}
		  }

	}

	
}