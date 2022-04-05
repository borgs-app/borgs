# Borgs

This project deployed: https://borgs.app/

## Introduction

Borgs is an ERC721 non-fungible token which can be deployed to any network that supports Solidity. Our smart contract generates and allows for the breeding of NFTs completely on chain - no side project required! The protocol serves as an open standard to be used or build upon for generative art on the blockchain and anyone is free to take this project and use their own art to generate similar projects. This project contains:

- The ERC721 Borgs contract
- A hosted service which listens for Borg events and copies them to a database for quick access 
- A sync for backdated events
- An API which provides public access to view borgs
- The Borgs contract itself
- An uploader that populates the contract ready for generations (although you must provide your own images!)

If you have any questions, please reach out at [https://discord.gg/7zEd995C](https://discord.gg/7zEd995C)

## Smart contracts

There are 3 contracts contained within:

- The generative Borgs contract (the ERC721)
- A Borgs shop contract
- A Borgs giveaway contract

Contract | Network | Address | Link to Polyscan
--- | --- | --- | --- |
Test Borgs | Polygon Mumbai | 0x83fe03b16096266ecd4b6422e3bc2ef25dd401a6 | https://mumbai.polygonscan.com/address/0x83fe03b16096266ecd4b6422e3bc2ef25dd401a6 |
Production Borgs | Polygon Mainnet | 0xa88e5cfa0257460490ce54052b4faee1b3d7f410 | https://polygonscan.com/address/0xa88e5cfa0257460490ce54052b4faee1b3d7f410 |

The source code for this can be found within.

## Life cycle - Borgs

The contract uses layers built up of layer items (also known as attributes). The layer items are made up from colors-positions. The contract will have no layers/items upon creation and require them to be added. For this it may be useful to refer to the BorgImageReader project contained within. 

The setup flow has been outlined in the diagram below:

![image](https://user-images.githubusercontent.com/7746153/161687043-0087cf74-b669-4bb0-a271-64ab5511afc5.png)

The above example shows the creation of:

- 1 layer
- 1 layer item
- 2 colors for layer item

### Add Layer

```solidity
function addLayer(string memory layerName) external onlyOwner editable
```

### Add Layer Item (attribute)

```solidity
function addLayerItem(string memory layerName, uint256 chance, string memory borgAttributeName) external onlyOwner editable
```

### Add Color to Layer Item

```solidity
function addColorToBorgAttribute(string memory borgAttributeName, string memory color, uint256[] memory positions) external onlyOwner editable
```

Borgs have 10 layers and a range of between 4-20 layer items per layer, this being said it is no limit! It is also important to note that the contract requires locking for edit before a user can interact with the contract.

### Locking for Edit

```solidity
function lock() external onlyOwner
```

Once the contract has been setup and locked for edit (which can not be undone once done), then borgs can be generated. The attributes for the generated borg are selected using a pseudo-random number which is computed by hashing the previous blockhash and an incrementing counter behind the scenes. Seed generation is not truly random - it can be predicted if you know the previous block's blockhash but it will be hard to determine which attributes will be generated since the chances for these are hidden from public view (attributes are weighted random) and the attributes are not entered in alphabetical order. 

![Generate Borg](https://user-images.githubusercontent.com/7746153/138068936-d2048fa7-d88c-4826-82fa-3daef6f02c5b.png)

### Generate Borg

```solidity
function generateBorg() external payable usable returns(uint256)
```

Once more than 1 borg has been generated it is then possible to breed the borgs (as long as both are owned by the persion trying to breed)

![Breed Borg](https://user-images.githubusercontent.com/7746153/138069714-4a6266fc-aeb7-49d7-a551-a43323d64af9.png)

### Breed Borgs

```solidity
function breedBorgs(uint256 borgId1, uint256 borgId2) external usable returns(uint256)
```

### Getting a Borg

Since the code to generate/show Borgs is all hosted on the blockchain, there needs to be a way to get the Borg. It is returned as a 1 dimensional array which needs to be converted into a 2 dimensional image after retrieval. 

![Get Borg](https://user-images.githubusercontent.com/7746153/138071321-9f3c91f2-1240-4d46-a804-d0c54ea171d9.png)

```solidity
function getBorg(uint256 borgId) public view returns(string memory name, string[] memory image, string[] memory attributes, uint256 parentId1, uint256 parentId2, uint256 childId)
```

The code to convert this 1D array into an image has been provided below in a few different languages:

### Javascript
```javascript
drawImage(imgHexValues) {
     var canvas = this.$el,
     ctx = canvas.getContext('2d'),
     width = 24,
     height = 24,
     scale = this.scale,
     sqrt = Math.sqrt(imgHexValues.length);

     ctx.clearRect(0, 0, canvas.width, canvas.height);
     canvas.width = width * scale;
     canvas.height = height * scale;

     imgHexValues.forEach((value, index) => {
     let xCoord = Math.floor(index % sqrt),
     yCoord = Math.floor(index / sqrt);

     ctx.fillStyle = value.length > 1 ? `#${value.substr(-6)}${value.substr(0, 2)}` : `#00000000`;
     ctx.fillRect(xCoord * scale, yCoord * scale, scale, scale);
     });
}
```

### C#

```CSharp
/// <summary>
/// This is used to convert an array of hex pixels back into an argb image
/// </summary>
/// <param name="hexValues">The string hex values to convert to image (flat)</param>
/// <returns>A bitmap</returns>
public static Bitmap ConvertBorgToBitmap(List<string> hexValues)
{
    // Assuming regular image ie. 24x24, 12x12, 48x48 etc.
    var sqrt = (int)Math.Sqrt(hexValues.Count());
    var bitmap = new Bitmap(sqrt, sqrt, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    for (int i = 0; i < hexValues.Count(); i++)
    {
        // Default white
        var pixel = Color.White;

        // If specified then isn’t white
        if (!string.IsNullOrEmpty(hexValues[i]))
        {
            var convertedHexValue = Convert.ToInt32(hexValues[i], 16);
            pixel = Color.FromArgb(convertedHexValue);
        }

        // Define 2d coords
        var y = i / sqrt;
        var x = i % sqrt;

        // Set in place
        bitmap.SetPixel(x, y, pixal);
    }

    // Return the built up image
    return bitmap;
}
```

### Java

```Java
/// <summary>
/// This is used to convert an array of hex pixels back into an argb image
/// </summary>
/// <param name="hexValues">The string hex values to convert to image (flat)</param>
/// <returns>A buffered image</returns>
public static BufferedImage convertBorgToBufferedImage(String[] hexValues)
{
     // Assuming regular image ie. 12x12, 24x24, 36x36 etc.
     int sqrt = (int)Math.sqrt(hexValues.length);
     BufferedImage image = new BufferedImage(sqrt, sqrt, BufferedImage.TYPE_INT_ARGB);

     for (int i = 0; i < hexValues.length; i++)
     {
         // Default white
         var pixel = Color.WHITE;

         // If specified then isn’t white
         if (hexValues[i] != null && !hexValues[i].trim().isEmpty())
             pixel = Color.decode(hexValues[i]);

         // Define 2d coords
         int y = i / sqrt;
         int x = i % sqrt;

         // Set in place
         image.setRGB(x, y, pixal.getRGB());
     }

     // Return the built up image
     return image;
}
```

## 

## Life cycle - Shop

### Sales

The Borgs can also be sold/purchased via this same contract. If you provide the create listing call with a 0x address then it is possible for anyone to buy it (public sale) but if the address is any address other than this, then only this address can purchase the listing (private sale), the worksflow for this has been provided below. There is a fee of 3% hard coded into the contract as commission (immutable), this however only gets deducted after the sale has been made meaning that you can cancel the sale with no fees required.

![Private Sale](https://user-images.githubusercontent.com/7746153/138070382-08d078dc-1aa3-481f-a812-99bb48571625.png)

```solidity
function addListing(uint256 tokenId, uint256 price, address buyer) public
```

```solidity
function purchaseListing(uint256 tokenId) public payable
```

There is also a way to remove a listing if the seller is getting cold feet (this can however only be performed by the seller themselves).

![Remove Listing](https://user-images.githubusercontent.com/7746153/138070896-60b1a273-877b-426a-a5d7-c4d655f823b4.png)

```solidity
function removeListing(uint256 tokenId) public
```

