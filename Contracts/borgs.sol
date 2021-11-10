// SPDX-License-Identifier: MIT

pragma solidity 0.8.7;

import 'github.com/OpenZeppelin/openzeppelin-contracts/blob/master/contracts/token/ERC721/extensions/ERC721Enumerable.sol';
import 'github.com/OpenZeppelin/openzeppelin-contracts/blob/master/contracts/access/Ownable.sol';
import 'github.com/OpenZeppelin/openzeppelin-contracts/blob/master/contracts/utils/Strings.sol';
import 'github.com/OpenZeppelin/openzeppelin-contracts/blob/master/contracts/utils/Address.sol';

/**
 * @dev Interface of the Ownable modifier handling contract ownership
 */
abstract contract ContractStates is Ownable {
    /**
    * @dev If the contract is editable
    */
    bool internal _isEditable;
    
    /**
     * @dev Sets the owner upon contract creation
     **/
    constructor() {
      _isEditable = true;
    }
  
    modifier editable() {
      require(_isEditable == true);
      _;
    }
    
    modifier usable() {
      require(_isEditable == false);
      _;
    }
    
    function lock() external onlyOwner{
        _isEditable = false;
    }
}

contract CommonObjects{
    
    struct BorgAttribute{
        bool exists;
        bytes8[] colors;
        uint256 colorSize;
        mapping (bytes8 => uint256[]) colorPositions;
    }

    struct Layer{
        bool exists;
        mapping(uint => LayerItem) layerItems;
        uint256 layerItemSize;
        uint256 layerItemsCumulativeTotal;
    }
    
    struct LayerItem{
        bool exists;
        uint256 chance;
        string borgAttributeName;
    }
    
    struct Borg{
        uint256 id;
        string name;
        bool exists;
        uint256 descendantId;
        string[] borgAttributeNames;
        uint256[] borgAttributeLayerPositions;
        uint256 borgAttributesSize;
        uint256 parentId1;
        uint256 parentId2;
    }

    struct CreateLayerItems{
        uint256 chance;
        uint8 layerIndex;
        string attributeName;
    }
}

contract NumberUtils{
    // Intializing the state variable
    uint _randNonce = 0;
          
    function _getRandomNumber(uint256 modulus) internal returns(uint){
           // increase nonce
           _randNonce++;  
           return uint(keccak256(abi.encodePacked(block.timestamp, 
                                                  msg.sender, 
                                                  _randNonce))) % modulus;
    }
}

contract Borgs is ERC721Enumerable, IERC721Receiver, ContractStates, CommonObjects, NumberUtils{
    
    // Define string library
    using Strings for string;
    
    // Mapped borg attributes (image:name)
    mapping (string => BorgAttribute) private _borgAttributes;
    
    // Mapped layers (name/position:layer data)
    mapping (uint8 => Layer) private _layers;
    
    // Mapped borg (id:borg)
    mapping (uint256 => Borg) private _borgs;
    
    // Mapped attribute usage (attribute name:usage)
    mapping (string => uint256) private _borgAttributesUsed;
    
    // The free whitelist
    mapping(address => bool) private _whitelist;
    
    // Borgs start from id1
    uint256 private _currentBorgId = 1;
    
    uint8 _layerCount;
    
    // How many have been generated
    uint256 private _currentGeneratedCount = 0;
    
    // How many have been generated
    uint256 private _currentFreeGeneratedCount = 0;
    
    // The base token uri
    string private _baseTokenURI;
    
    // How many have been generated
    uint256 private _currentBredCount = 0;
    
    // Max supply (can ever be generated)
    uint256 private immutable SUPPLY_LIMIT = 20000;
    
    // The size of image to produce (sets output array size)
    uint256 public immutable IMAGE_SIZE = 576;
    
    // The cost to call generateBorgs (wei)
    uint256 public immutable GENERATION_COST = 1000;

    // Number of free calls to generateBorgs
    uint256 public immutable FREE_GENERATED_COUNT = 99;

    // Event for borg generation
    event GeneratedBorg(uint256 indexed borgId, address indexed creator, uint256 timestamp);
    
    event BredBorg(uint256 indexed childId, uint256 indexed parentId1, uint256 indexed parentId2, address breeder, uint256 timestamp);
    
    // Set name, shortcode and limit on construction
    constructor(string memory name, string memory symbol) ERC721(name, symbol){
    }
    
    /**
     * @dev See {IERC721Receiver-onERC721Received}.
     *
     * Always returns `IERC721Receiver.onERC721Received.selector`.
     */
    function onERC721Received(
        address,
        address,
        uint256,
        bytes memory
    ) public virtual override returns (bytes4) {
        return this.onERC721Received.selector;
    }
    
    // Set the base token uri for opensea
    function setBaseTokenURI(string memory newBaseTokenURI) external onlyOwner returns (string memory) {
        return _baseTokenURI = newBaseTokenURI;
    }
    
    // Get the base token uri (for opensea)
    function baseTokenURI() public view returns (string memory) {
        return _baseTokenURI;
    }
    
    // Get a uri for a token
    function tokenURI(uint256 tokenId) public override view returns (string memory) {
        return string(abi.encodePacked(_baseTokenURI, Strings.toString(tokenId)));
    }
    
    // Get the current bred counter
    function getCurrentBredCount() public view returns(uint256){
        return _currentBredCount;
    }
    
    // Get the current generated counter
    function getCurrentGenerationCount() public view returns(uint256){
        return _currentGeneratedCount;
    }
    
    function addToWhitelist(address whitelistAddress) external onlyOwner{
        _whitelist[whitelistAddress] = true;
    }
    
    function removeFromWhitelist(address whitelistAddress) external onlyOwner{
        _whitelist[whitelistAddress] = false;
    }
    
    function getClaimedFreeBorgCount() public view returns (uint256){
        return _currentFreeGeneratedCount;
    }

    function getBorg(uint256 borgId) public view returns(string memory name, bytes8[] memory image, string[] memory attributes, uint256 parentId1, uint256 parentId2, uint256 childId){
        // Find the borg in question
        Borg memory borg = _borgs[borgId];
        
        // Combine the attributes to form array of hex colors
        image = combineBorgAttributes(borg.borgAttributeNames);
        
        // Set parents
        parentId1 = borg.parentId1;
        parentId2 = borg.parentId2;
        
        // Set child
        childId = borg.descendantId;
        
        // Set attributes
        attributes = borg.borgAttributeNames;
        
        // Set name
        name = borg.name;
    }
    
    function getAttributesUsedCount(string[] memory attributes) public view returns(uint256[] memory){
        uint256[] memory attributeCountsUsed = new uint256[](attributes.length);
        
        for(uint256 i=0;i<attributes.length;i++){
            attributeCountsUsed[i] = _borgAttributesUsed[attributes[i]];
        }
        // Return the count of attributes used
        return attributeCountsUsed;
    }
    
    function setLayers(uint8 layerCount) external onlyOwner editable{
        // Set total layer count
        _layerCount = layerCount;
        
        // Add layers
        for(uint8 i=0;i<layerCount;i++){
            Layer storage layer = _layers[i];
            layer.exists = true;
        }
    }
    
    function addLayerItems(CreateLayerItems[] calldata createLayerItems) external onlyOwner editable{
        // Add all the layer items
        for(uint256 i =0;i<createLayerItems.length;i++){
            // Get the layer
            Layer storage layer = _layers[createLayerItems[i].layerIndex];
            
            // Basic check
            require(layer.exists, 'Layer doesnt exist');
            
            // Create a new item to add to layer
            LayerItem memory item = LayerItem(true, createLayerItems[i].chance, createLayerItems[i].attributeName);
            
            // Add onto end
            layer.layerItems[layer.layerItemSize] = item;
            
            // Update the item size
            layer.layerItemSize += 1;
            
            // Update the cumulative total of chances for the layer
            layer.layerItemsCumulativeTotal += createLayerItems[i].chance;
        }
    }
    
    function addBlankToLayers(string[] calldata attributeNames) external onlyOwner editable{
        for(uint256 i=0;i<attributeNames.length;i++){
            // Create attribute
             // Get the item to change in storage
            BorgAttribute storage borgAttribute = _borgAttributes[attributeNames[i]];
            borgAttribute.exists = true;   
        }
    }
    
    function createBorgAttribute(string calldata borgAttributeName, bytes8[] calldata colors, uint256[][] calldata positions) external onlyOwner editable{
        uint256 colorLength = colors.length;
        
        // Reauire positions have been provided for the colours
        require(colorLength == positions.length, 'Color/position length must be the same');
        
        // Add each color
        for(uint256 i =0;i<colorLength;i++){
            // Get the item to change in storage
            BorgAttribute storage borgAttribute = _borgAttributes[borgAttributeName];
            
            // Get the color
            bytes8 color = colors[i];
            
            // Add the color
            borgAttribute.colors.push(color);
            borgAttribute.colorSize += 1;
            borgAttribute.colorPositions[color] = positions[i];
            borgAttribute.exists = true;   
        }
    }
    
    function nameBorg(uint256 borgId, string memory newName) public {
        // Check the owner is the only one renaming
        require(msg.sender == ownerOf(borgId), "Borg must be owned to rename");
        
        // Get the borg and check exists
        Borg storage borg = _borgs[borgId];
        require(borg.exists, "Borg doesn't exist");  
        
        // Rename Borg
        borg.name = newName;
    }
    
    function recoverFunds(address payable toAddress, uint256 amount) public onlyOwner{
        toAddress.transfer(amount);
    } 
    
    function recoverBorg(address payable toAddress, uint256 borgId) public onlyOwner{
        safeTransferFrom(address(this), toAddress, borgId);
    } 
    
    function getGenerationPrice() public view returns(uint256){
        // If caller is whitelisted and the free generation count hasnt been reached, then its 0 cost
        if(_whitelist[msg.sender] == true && (_currentFreeGeneratedCount < FREE_GENERATED_COUNT))
            return 0;
 
        // Otherwise its the standard price
        return GENERATION_COST;
    }
    
    function generateBorg() external payable usable returns(uint256){
        // Check layers exist
        require(_layerCount > 0, "No layers present");
        
        // Check that enough value to cover cost has been sent with request or the user is a whitelisted address (owner)
        if(_whitelist[msg.sender] == true && (_currentFreeGeneratedCount < FREE_GENERATED_COUNT))
        {
            // Ensure no value was sent
            require(msg.value == 0, "The calling address is whitelisted and can still claim free borgs");
            
            // Up the free generated count
            _currentFreeGeneratedCount = _currentFreeGeneratedCount + 1;
        }
        else
        {
            // Check the user has supplied generation cost
            require(msg.value == GENERATION_COST, "Value provided needs to equal generation cost");
            
            // Check we havent reached the limit for generation (finite supply)
            require(SUPPLY_LIMIT >= (_currentGeneratedCount + FREE_GENERATED_COUNT), "Borg generation limit has been reached");
        }
        
        // Get the total layer count
        uint8 layerCount = _layerCount;
        
        // Init the selected attribute arrays
        string[] memory borgAttributeNames = new string[](layerCount);
        uint256[] memory layerItemPositions = new uint256[](layerCount);

        // For each of the layers available, select a random item from it
        for(uint8 i=0;i<layerCount;i++){
            // Get a random item from the layer
            (borgAttributeNames[i], layerItemPositions[i]) = _getRandomLayerItemName(i);
            
            // Set in usage recorder
            _updateBorgAttributesUsed(borgAttributeNames[i]);
        }
        
        // Create the borg
        uint256 borgId = _createBorg(borgAttributeNames, layerItemPositions, 0, 0);
 
        // The borg and the token are 1:1 as the ids are the same
        _safeMint(msg.sender, borgId);
        
        // Up the generated count
        _currentGeneratedCount = _currentGeneratedCount + 1;
        
        // Fire event
        emit GeneratedBorg(borgId, msg.sender, block.timestamp);
        
        return borgId;
    }
    
    function breedBorgs(uint256 borgId1, uint256 borgId2) external usable returns(uint256){
        // Get the first borg to breed
        Borg storage borg1 = _borgs[borgId1];
        require(borg1.exists, 'Borg 1 doesnt exist');
        require(borg1.descendantId < 1, 'Borg1 already has a descendant');
        
        // Get the second borg to breed
        Borg storage borg2 = _borgs[borgId2];
        require(borg2.exists, 'Borg 2 doesnt exist');
        require(borg2.descendantId < 1, 'Borg2 already has a descendant');
        
        // Check to breed the same
        require(borgId1 != borgId2, 'Cannot breed the same borg more than once');
        
        // Require caller is owner of both borgs
        require(ownerOf(borgId1) == msg.sender, 'Must be owner borg 1');
        require(ownerOf(borgId2) == msg.sender, 'Must be owner borg 2');
        
        // Check the attributes size is the same
        require(borg1.borgAttributeNames.length == borg2.borgAttributeNames.length, 'Borg layer counts do not match');
        
        // Burn parents
        _burn(borgId1);
        _burn(borgId2);
        
        // Select the borgs peices it is made up from (rareset from each)
        uint256[] memory layerItemPosition1 = borg1.borgAttributeLayerPositions;
        uint256[] memory layerItemPosition2 = borg2.borgAttributeLayerPositions;
        
        // Filter out the rareset of the 2 sets of items (layer by layer)
        (uint256[] memory borgAttributeLayerPositions, string[] memory borgAttributeNames) = _filterRarestBorgPeices(layerItemPosition1, layerItemPosition2);
        
        // Build the borg
        uint256 borgId = _createBorg(borgAttributeNames, borgAttributeLayerPositions, borgId1, borgId2);

        // Mint token to attach borg to
        _safeMint(msg.sender, borgId);
        
        // Set the parents new descendant
        borg1.descendantId = borgId;
        borg2.descendantId = borgId;
        
        // Up the bred count
        _currentBredCount = _currentBredCount + 1;
        
        // Fire events (1 for each parents)
        emit BredBorg(borgId, borgId1, borgId2, msg.sender, block.timestamp);
        
        // Return new borgs/tokens id
        return borgId;
    }
    
    function previewBreedBorgs(uint256 borgId1, uint256 borgId2) external view usable returns(bytes8[] memory image, string[] memory attributes){
        // Get the first borg to breed
        Borg storage borg1 = _borgs[borgId1];
        require(borg1.exists, 'Borg 1 doesnt exist');
        require(borg1.descendantId < 1, 'Borg1 already has a descendant');
        
        // Get the second borg to breed
        Borg storage borg2 = _borgs[borgId2];
        require(borg2.exists, 'Borg 2 doesnt exist');
        require(borg2.descendantId < 1, 'Borg2 already has a descendant');
        
        // Check the attributes size is the same
        require(borg1.borgAttributeNames.length == borg2.borgAttributeNames.length, 'Borg layer counts do not match');
        
        // Select the borgs peices it is made up from (rareset from each)
        uint256[] memory layerItemPosition1 = borg1.borgAttributeLayerPositions;
        uint256[] memory layerItemPosition2 = borg2.borgAttributeLayerPositions;
        
        // Filter out the rareset of the 2 sets of items (layer by layer)
        (,string[] memory borgAttributeNames) = _filterRarestBorgPeicesForPreview(layerItemPosition1, layerItemPosition2);

        // Create image
        image = combineBorgAttributes(borgAttributeNames);
        attributes = borgAttributeNames;
    }

    function _filterRarestBorgPeices(uint256[] memory layerItemPosition1, uint256[] memory layerItemPosition2) internal returns(uint256[] memory rarestLayerPositions, string[] memory borgPeiceNames){
        // Check the peices size is the same
        require(layerItemPosition1.length == layerItemPosition1.length, 'Borg layer counts do not match');
        
        // Create the new item arrays
        rarestLayerPositions = new uint256[](layerItemPosition1.length);
        string[] memory rarestBorgAttributeNames = new string[](layerItemPosition1.length);
        
        // For each layer we select the rarest of the 2 items
        for(uint8 i=0;i<_layerCount;i++){
            // From that get the layer
            Layer storage layer = _layers[i];
            
            // Get items from both parents
            LayerItem storage layerItem1 = layer.layerItems[layerItemPosition1[i]];
            LayerItem storage layerItem2 = layer.layerItems[layerItemPosition2[i]];
            
            // Compare and take the item with the lowst chance
            if(layerItem1.chance <= layerItem2.chance){
                rarestLayerPositions[i] = layerItemPosition1[i];
                rarestBorgAttributeNames[i] = layerItem1.borgAttributeName;
            }
            else{
                rarestLayerPositions[i] = layerItemPosition2[i];
                rarestBorgAttributeNames[i] = layerItem2.borgAttributeName;
            }
            
            // Set in usage recorder
            _updateBorgAttributesUsed(rarestBorgAttributeNames[i]);
        }
        
        // Return the rarest positions
        return (rarestLayerPositions, rarestBorgAttributeNames);
    }
    
    function _filterRarestBorgPeicesForPreview(uint256[] memory layerItemPosition1, uint256[] memory layerItemPosition2) internal view returns(uint256[] memory rarestLayerPositions, string[] memory borgPeiceNames){
        // Create the new item arrays
        rarestLayerPositions = new uint256[](layerItemPosition1.length);
        string[] memory rarestBorgAttributeNames = new string[](layerItemPosition1.length);
        
        // For each layer we select the rarest of the 2 items
        for(uint8 i=0;i<_layerCount;i++){
            // From that get the layer
            Layer storage layer = _layers[i];
            
            // Get items from both parents
            LayerItem storage layerItem1 = layer.layerItems[layerItemPosition1[i]];
            LayerItem storage layerItem2 = layer.layerItems[layerItemPosition2[i]];
            
            // Compare and take the item with the lowst chance
            if(layerItem1.chance <= layerItem2.chance){
                rarestLayerPositions[i] = layerItemPosition1[i];
                rarestBorgAttributeNames[i] = layerItem1.borgAttributeName;
            }
            else{
                rarestLayerPositions[i] = layerItemPosition2[i];
                rarestBorgAttributeNames[i] = layerItem2.borgAttributeName;
            }
        }
        
        // Return the rarest positions
        return (rarestLayerPositions, rarestBorgAttributeNames);
    }
    
    function _updateBorgAttributesUsed(string memory borgAttributeName) internal{
        // Get the peice to update count of
        uint256 amount = _borgAttributesUsed[borgAttributeName];
        
        // Update value
        _borgAttributesUsed[borgAttributeName] = amount + 1;
    }
    
    function combineBorgAttributes(string[] memory borgAttributeNames) public view returns (bytes8[] memory hexPixals){
        // Init the 
        hexPixals = new bytes8[](IMAGE_SIZE);
        
        for(uint256 i=0;i<borgAttributeNames.length;i++){
             BorgAttribute storage borgAttribute = _borgAttributes[borgAttributeNames[i]];
 
             for(uint256 j = 0;j<borgAttribute.colors.length;j++){
                bytes8 color = borgAttribute.colors[j];
                uint256[] memory positions = borgAttribute.colorPositions[color];
                for(uint256 k = 0;k<positions.length;k++){
                    hexPixals[positions[k]] = color;
                }
            }
        }
 
        return hexPixals;
    }
    
    function _getRandomLayerItemName(uint8 layerNumber) internal returns(string memory borgAttributeName, uint256 position){
        // Get the layer to select item of
        Layer storage layer = _layers[layerNumber];
        
        // Basic checks
        require(layer.exists, "No layer was found");
        require(layer.layerItemSize > 0, "No layer was found");
        
        // Get a random number from 0-the cumulative total of all layer item chances
        uint256 randomChance = _getRandomNumber(layer.layerItemsCumulativeTotal);
        
        // Define the cumulative total to be used as we iterate
        uint256 cumulativeChance = 0;
        
        // Iterate over the layer items until we reach the cumulativeChance (weighted random)
        for(uint256 i=0;i<layer.layerItemSize;i++){
            // Get the layer item
            LayerItem memory item = layer.layerItems[i];
            
            // add the chance for the item to the running total
            cumulativeChance += item.chance;
            
            // If the running total is greater than the random number, we have a winner
            if(cumulativeChance >= randomChance){
                // Set return values
                borgAttributeName = item.borgAttributeName;
                position = i + 0;
                
                // Leave loop
                i = layer.layerItemSize;
            }
        }
    }
    
    function _createBorg(string[] memory borgAttributeNames, uint256[] memory layerItemPositions, uint256 parentId1, uint256 parentId2) internal returns(uint256){
        // Get a new id
        uint256 borgId = _currentBorgId++;
        
        // Get the storage item to fill
        Borg storage borg = _borgs[borgId];
        
        // Set props
        borg.id = borgId;
        borg.exists = true;
        borg.borgAttributesSize = _layerCount;
        borg.borgAttributeNames = borgAttributeNames;
        borg.borgAttributeLayerPositions = layerItemPositions;
        borg.parentId1 = parentId1;
        borg.parentId2 = parentId2;
        
        // Return generated id
        return borgId;
    }
}