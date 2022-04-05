# Borgs

This project deployed: https://borgs.app/

## Introduction

Borgs is an ERC721 non-fungible token which can be deployed to any network that supports Solidity. Our smart contract generates and allows for the breeding of NFTs completely on chain - no side project required! The protocol serves as an open standard to be used or build upon for generative art on the blockchain and anyone is free to take this project and use their own art to generate similar projects. This project contains:

- Solidity contracts
- MSSQL Database for API
- .NET Core 3.1 hosted service which listens for Borg events and copies them to a database for quick access 
- .NET Core 3.1 sync for backdated events
- .NET Core 3.1 API which provides public access to view borgs
- A simple .NET Core console application that populates the contract ready for generations (although you must provide your own images!)

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

![image](https://user-images.githubusercontent.com/7746153/161687760-867d2610-67cb-46b2-809c-224024517e06.png)

### Set Layers

The setup of the different layers

```solidity
function addLayer(string memory layerName) external onlyOwner editable
```

*Example*: 12

### Add Blanks

Adds all blank attributes in 1 fell swoop. 

```solidity
function addBlanks(string[] attributeNames) external onlyOwner editable
```

*Example*: ["blank1", "blank2", ..]

### Create Borg Attributes

Creates a borg attributre which can later be linked to a layer. The attribute is made up of colors and position (index/es) of those colors in the 1d output image. It is to be noted that the hexColors index is matched to the index of the positions ie. hexColor[2] will use positions[2]. 

```solidity
function createBorgAttribute(string borgAttributeName, byte[][] hexColors, uint256[][] positions) external onlyOwner editable
```

*Example*: "face_blue", [[70,70,48,48,48,48,48,48], [70,48,48,48,48,48,48,48]], [[30,36,38,..],[12,22,43,..]]

### Add Attributes To Layers

Finally link up the attributes to layers (and define their chances). Borgs have 10 layers and a range of between 4-20 layer items per layer, this being said it is no limit!

```solidity
function addLayerItems(CreateLayerItems[] layerItems) external onlyOwner editable
```

*Example*: [{Chance:15,LayerIndex:0,"face_blue"}, ..]

### Locking for Edit

Once the contract has been setup and locked for edit (which can not be undone once done), then borgs can be generated. 

```solidity
function lock() external onlyOwner
```

### Generate Borg

A random Borg can be generated using the method call below. The attributes for the generated borg are selected using a pseudo-random number which is computed by hashing the previous blockhash and an incrementing counter behind the scenes. Seed generation is not truly random - it can be predicted if you know the previous block's blockhash but it will be hard to determine which attributes will be generated since the chances for these are hidden from public view (attributes are weighted random) and the attributes are not entered in alphabetical order. 

```solidity
function generateBorg() external payable usable returns(uint256)
```

### Breed Borgs

Once more than 1 borg has been generated it is then possible to breed the borgs (as long as both are owned by the persion trying to breed).

```solidity
function breedBorgs(uint256 borgId1, uint256 borgId2) external usable returns(uint256)
```

*Example*: 2,3

### Getting a Borg

Since the code to generate/show Borgs is all hosted on the blockchain, there needs to be a way to get the Borg. It is returned as a 1 dimensional array which needs to be converted into a 2 dimensional image after retrieval. 

```solidity
function getBorg(uint256 borgId) public view returns(string memory name, string[] memory image, string[] memory attributes, uint256 parentId1, uint256 parentId2, uint256 childId)
```

*Example*: 2

The code to convert the 1D array into an image has been provided below in a few different languages:

#### Javascript
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

#### C#

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

#### Java

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

