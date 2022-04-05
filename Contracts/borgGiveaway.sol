// SPDX-License-Identifier: MIT

pragma solidity 0.8.7;

import '@openzeppelin/contracts/token/ERC721/extensions/IERC721Enumerable.sol';
import '@openzeppelin/contracts/access/Ownable.sol';
import '@openzeppelin/contracts/utils/Strings.sol';
import '@openzeppelin/contracts/utils/Address.sol';

/**
 * @dev Interface of the Ownable modifier handling contract ownership
 */
abstract contract ContractStates is Ownable {

    bool internal _isActive;
    
    /**
     * @dev Sets the owner upon contract creation
     **/
    constructor() {
      _isActive = true;
    }

    modifier active() {
       require(_isActive == true);
       _;
    }

    function deactivate() external onlyOwner{
        _isActive = false;
    }
}

contract NumberUtils{
    function _getRandomNumber(uint256 seed, uint256 modulus) internal view returns(uint){
           return uint(keccak256(abi.encodePacked(seed, 
                                                  msg.sender
                                                  ))) % modulus;
    }
}

contract BorgsGiveaway is ContractStates, NumberUtils{
    
    // Define string library
    using Strings for string;

    // The addressses
    address[] private _addresses;

    // Addresses already added
    mapping(address => bool) private _addedAddresses; 

    // The borgs contract
    IERC721Enumerable private _borgs;

    // Setup the contract in the constructor
    constructor(address contractAddress){
        _borgs = IERC721Enumerable(contractAddress);
    }

    function addAddress() active public{
       // Check if address has already been added
       require(_addedAddresses[msg.sender] == false, "This address has already been addded");

       // Add address to blacklist
       _addedAddresses[msg.sender] = true;
       
       // Add address
       _addresses.push(msg.sender);
    }

    function payPeople(uint256 seed) onlyOwner active public {
       // Check there are addresses to pay
       require(_addresses.length > 0, "There is noone to payout to");

       // Get the balance of this contract (what to give away)
       uint256 balance = getBorgBalance();

       // Get a random number
       uint256 randomNumber = _getRandomNumber(seed, _addresses.length);

       for(uint256 i = 0; i < balance; i++){
           // Get the a random position to find who to reward
           uint256 position = _getRandomNumber(randomNumber, _addresses.length-1);

           // Get the next address to reward
           address addressToReward = _addresses[position];

           // Get token to give away
           uint256 borgId = _borgs.tokenOfOwnerByIndex(address(this), i);

           // Transfer to address
           _borgs.transferFrom(address(this), addressToReward, borgId);
       }
    } 

    function getBorgBalance() public view returns (uint256){
       // Get the balance of this contract (what to give away)
       return _borgs.balanceOf(address(this));
    }

    function getRandomNumberTest(uint256 seed) public view returns (uint256){
        return _getRandomNumber(seed, _addresses.length);
    }
}