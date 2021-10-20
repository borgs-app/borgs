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

The contract uses layers built up of layer items (also known as attributes). The layer items are made up from colours-positions. The contract will have no layers/items upon creation and require them to be added. For this it may be useful to refer to the BorgImageReader project contained within. 

The setup flow has been outlined in the diagram below:

![Setup](https://user-images.githubusercontent.com/7746153/138066292-185cce2d-569d-4992-ac5f-131b86171ea8.png)

The above example shows the creation of:

### 1 layer

```solidity
function addLayer(string memory layerName) external onlyOwner editable
```

### 1 layer item

```solidity
function addLayerItem(string memory layerName, uint256 chance, string memory borgAttributeName) external onlyOwner editable
```

### 2 colours for the layer item

```solidity
function addColorToBorgAttribute(string memory borgAttributeName, string memory color, uint256[] memory positions) external onlyOwner editable
```

Borgs have 10 layers and a range of between 4-20 layer items per layer, this being said it is no limit! It is also important to note that the contract requires locking for edit before a user can interact with the contract.

### Locking for edit

```solidity
function lock() external onlyOwner
```

Once the contract has been setup and locked for edit (which can not be undone once done), then borgs can be generated. 

![Generate Borg](https://user-images.githubusercontent.com/7746153/138068936-d2048fa7-d88c-4826-82fa-3daef6f02c5b.png)






