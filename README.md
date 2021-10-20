# Borgs

## Introduction

Borgs is an all in one smart contract that generates, breeds and facilitates the sale of borgs. The protocol serves as an open standard to be used or build upon for generative art on the blockchain. Anyone is free to take this project and use their own art to generate similar projects. This project contains:

- A hosted service which listens for Borg events and copies them to a database for quick access 
- A sync for backdated events
- An API which provides public access to view borgs
- The Borgs contract itself
- An uploader that populates the contract ready for generations (although you must provide your own images!)

If you have any questions, please reach out at [https://discord.gg/7zEd995C](https://discord.gg/7zEd995C)

## Smart contract

The smart contract 

Contract | Network | Address | Link to Polyscan
--- | --- | --- | --- |
Test Contract | Polygon Mumbai | 0xaf20d38d7edf2314abf3a5e9fe54bf77f02879da | https://mumbai.polygonscan.com/address/0xaf20d38d7edf2314abf3a5e9fe54bf77f02879da |

The source code for this can be found within.

## Life cycle

The contract uses layers built up of layer items (also known as attributes). The layer items are made up from colors-positions. The contract will have no layers/items upon creation and require them to be added. For this it may be useful to refer to the BorgImageReader project contained within. 

The setup flow has been outlined in the diagram below:

![Setup](https://user-images.githubusercontent.com/7746153/138066292-185cce2d-569d-4992-ac5f-131b86171ea8.png)

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

Once the contract has been setup and locked for edit (which can not be undone once done), then borgs can be generated. 

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

### Sales

The Borgs can also be sold/purchased via this same contract. If you provide the create listing call with a 0x address then it is possible for anyone to buy it (public sale) but if the address is any address other than this, then only this address can purchase the listing (private sale), the worksflow for this has been provided below.

![Private Sale](https://user-images.githubusercontent.com/7746153/138070382-08d078dc-1aa3-481f-a812-99bb48571625.png)




