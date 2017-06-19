# Calculate the GTIN-13 (EAN-13) barcode checksum digit

as a string...

```csharp
    public static class Barcode
    {
        public static string Gtin13Checksum(string gtin)
        {
            if (string.IsNullOrEmpty(gtin) || gtin.Length != 12)
            {
                throw new ArgumentException("GTIN must be exactly 12 characters", nameof(gtin));
            }
            
            var digits = new int[gtin.Length];
            for (var i = 0; i < digits.Length; i++)
            {
                digits[i] = int.Parse(gtin[i].ToString());
            }

            var threes = (digits[1] + digits[3] + digits[5] + digits[7] + digits[9] + digits[11]) * 3;
            var ones = digits[0] + digits[2] + digits[4] + digits[6] + digits[8] + digits[10];
            var checkValue = (10 - (threes + ones) % 10) % 10;

            return checkValue.ToString();
        }
    }
```

To use:

```csharp
    public class BarcodeTests
    {
        [Fact]
        public void CanCreateGtin13Checksum()
        {
            var code = "629104150021";
            var checksum = Barcode.Gtin13Checksum(code);
            Assert.Equal("3", checksum);
        }
    }
```