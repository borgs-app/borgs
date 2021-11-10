// SPDX-License-Identifier: MIT

pragma solidity 0.8.7;

import '@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol';
import '@openzeppelin/contracts/access/Ownable.sol';

contract CommonObjects{
    
    struct Listing{
        uint256 borgId;
        bool exists;
        uint256 price;
        address seller;
        address buyer;
        uint256 timestamp;
    }
}

contract BorgShop is Ownable, IERC721Receiver, CommonObjects{
    
    // The borgs contract
    IERC721 _borgsContract;
    
    // The listings
    mapping (uint256 => Listing) private _listings;
    
    // Array with all listings token ids, used for enumeration
    uint256[] private _allTokenIdsInActiveListings;

    // Mapping from token id to position in the allListings array
    mapping(uint256 => uint256) private _allTokenIdsInActiveListingsIndex;
    
    // Mapping from owner to list of owned token IDs
    mapping(address => mapping(uint256 => uint256)) private _ownedListings;

    // Mapping from token ID to index of the owner tokens list
    mapping(uint256 => uint256) private _ownedListingsIndex;
    
    // Owners total listings
    mapping(address => uint256) private  _ownersTotalListings;
    
    // Sale cost (only taken once sale has been agreed)
    uint256 public immutable MARKETPLACE_PERCENT = 3;
    
    // The maximum sale pruce of a borg
    uint256 public immutable MAX_SALE_PRICE = 999999999000000000000000000;
    
    
    // Event for creating listing
    event CreatedListing(uint256 indexed borgId, address indexed seller, address indexed buyer, uint256 price, uint256 timestamp);
    
    // Event for removing listing
    event RemovedListing(uint256 indexed borgId, address indexed seller, uint256 timestamp);
    
    // Event for the purchase of a listing
    event PurchasedListing(uint256 indexed borgId, address indexed seller, address indexed buyer, uint256 price, uint256 timestamp);

    // Set name, shortcode and limit on construction
    constructor(address contractAddress) {
        _borgsContract = IERC721(contractAddress);
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
    
    function getMarketPlaceListingCommission(uint256 price) public pure returns(uint256){
        return (price/100)*MARKETPLACE_PERCENT;
    }
    
    function addListing(uint256 tokenId, uint256 price, address buyer) public{
        // Ensure caller is token owner
        require(_borgsContract.ownerOf(tokenId) == msg.sender, "Caller doesn't own Borg");
        
        // Ensure the amount is less than the max price
        require(price < MAX_SALE_PRICE, 'Maximum sale price exceeded');
        
        // Transfer borg to Contract
        _borgsContract.safeTransferFrom(msg.sender, address(this), tokenId);
        
        // Set listing
        Listing memory listing = Listing(tokenId,true,price,msg.sender,buyer,block.timestamp);
        _listings[tokenId] = listing;
        
        // Add to the all list
        _addListingToAllListingsEnumeration(tokenId);
        
        // Add to owner list
        _addListingToOwnerEnumeration(msg.sender, tokenId);
        
        // Create event
        emit CreatedListing(tokenId, msg.sender, buyer, price, block.timestamp);
    }
    
    function removeListing(uint256 tokenId) public{
        // Get listing
        Listing storage listing = _listings[tokenId];
        
        // Checks for listing to exist
        require(listing.exists, "Listing is not active or doesn't exist");
        
        // Checks caller is owner of listing
        require(listing.seller == msg.sender);
        
        // Remove listing
        listing.exists = false;
        
         // Remove from the all list
        _removeTokenFromAllListingsEnumeration(tokenId);
        
        // Remove from owner list
        _removeListingFromOwnerEnumeration(msg.sender, tokenId);
        
        // Transfer listing back to user
        _borgsContract.safeTransferFrom(address(this), listing.seller, tokenId);
        
        // Create event
        emit RemovedListing(tokenId, msg.sender, block.timestamp);
    }
    
    function purchaseListing(uint256 tokenId) public payable{
        // Get listing
        Listing storage listing = _listings[tokenId];
        
        // Checks for listing to exist
        require(listing.exists, "Listing is not active or doesn't exist");
        
        // Checks caller is the set buyer of listing (or no buyer has been set)
        require(listing.buyer == msg.sender || listing.buyer == address(0), "This is a private listing where you're not the buyer");
        
        // Check amount is enough to match listing
        require(listing.price == msg.value, "Price has not been met or has been exceeded");
        
        // Remove listing
        listing.exists = false;
        
        // Remove from the all list
        _removeTokenFromAllListingsEnumeration(tokenId);
        
        // Remove from owner list
        _removeListingFromOwnerEnumeration(listing.seller, tokenId);
                
        // Transfer money to seller (minus the market place fee)
        payable(listing.seller).transfer(listing.price - getMarketPlaceListingCommission(listing.price));  
        
        // Transfer token to Buyer
        _borgsContract.safeTransferFrom(address(this), msg.sender, tokenId);
        
        // Create event
        emit PurchasedListing(tokenId, listing.seller, msg.sender, listing.price, block.timestamp);
    }
    
    function getListingByTokenId(uint256 tokenId) public view returns(uint256 borgId, uint256 price, address seller, address buyer, uint256 timestamp){
        // Get listing
        Listing memory listing = _listings[tokenId];
        
        // Confirm listing exists
        require(listing.exists, "Listing doesn't exist");

        // Return all other listing data
        borgId = listing.borgId;
        price = listing.price;
        seller = listing.seller;
        buyer = listing.buyer;
        timestamp = listing.timestamp;
    }
    
    function getListingByIndex(uint256 index) public view returns(uint256 tokenId, uint256 price, address seller, address buyer, uint256 timestamp){
        // Get tokenId
        uint256 tokenAtIndex = getListingsTokenIdByIndex(index);
        
        // Return listing
        return getListingByTokenId(tokenAtIndex);
    }
    
    function totalTokensListed() public view virtual returns (uint256) {
        return _allTokenIdsInActiveListings.length;
    }

    function getListingsTokenIdByIndex(uint256 index) public view virtual returns (uint256) {
        return _allTokenIdsInActiveListings[index];
    }
    
    function getOwnersListingsTokenIdByIndex(address owner, uint256 index) public view virtual returns (uint256) {
        return _ownedListings[owner][index];
    }
    
    function getOwnersListingByIndex(address owner, uint256 index) public view returns(uint256 tokenId, uint256 price, address seller, address buyer, uint256 timestamp){
        // Get tokenId
        uint256 tokenAtIndex = getOwnersListingsTokenIdByIndex(owner, index);
        
        // Return listing
        return getListingByTokenId(tokenAtIndex);
    }
    
    function getOwnersTotalListings(address owner) public view virtual returns (uint256) {
        return _ownersTotalListings[owner];
    }
    
    function recoverFunds(address payable toAddress, uint256 amount) public onlyOwner{
        toAddress.transfer(amount);
    } 
    
    function recoverBorg(address payable toAddress, uint256 borgId) public onlyOwner{
        _borgsContract.safeTransferFrom(address(this), toAddress, borgId);
    } 
    
    function _addListingToAllListingsEnumeration(uint256 listingId) private {
        _allTokenIdsInActiveListingsIndex[listingId] = _allTokenIdsInActiveListings.length;
        _allTokenIdsInActiveListings.push(listingId);
    }

    function _removeTokenFromAllListingsEnumeration(uint256 tokenId) private {
        // To prevent a gap in the tokens array, we store the last token in the index of the token to delete, and
        // then delete the last slot (swap and pop).

        uint256 lastTokenIndex = _allTokenIdsInActiveListings.length - 1;
        uint256 tokenIndex = _allTokenIdsInActiveListingsIndex[tokenId];

        // When the token to delete is the last token, the swap operation is unnecessary. However, since this occurs so
        // rarely (when the last minted token is burnt) that we still do the swap here to avoid the gas cost of adding
        // an 'if' statement (like in _removeTokenFromOwnerEnumeration)
        uint256 lastTokenId = _allTokenIdsInActiveListings[lastTokenIndex];

        _allTokenIdsInActiveListings[tokenIndex] = lastTokenId; // Move the last token to the slot of the to-delete token
        _allTokenIdsInActiveListingsIndex[lastTokenId] = tokenIndex; // Update the moved token's index

        // This also deletes the contents at the last position of the array
        delete _allTokenIdsInActiveListingsIndex[tokenId];
        _allTokenIdsInActiveListings.pop();
    }
    
    function _addListingToOwnerEnumeration(address to, uint256 tokenId) private {
        uint256 length = _ownersTotalListings[to];
        
        _ownedListings[to][length] = tokenId;
        _ownedListingsIndex[tokenId] = length;
        
         // Adjust the users total count
        _ownersTotalListings[to] = length + 1;
    }
    
    function _removeListingFromOwnerEnumeration(address from, uint256 tokenId) private {
        // To prevent a gap in from's tokens array, we store the last token in the index of the token to delete, and
        // then delete the last slot (swap and pop).

        uint256 lastTokenIndex = _ownersTotalListings[from] - 1;
        uint256 tokenIndex = _ownedListingsIndex[tokenId];

        // When the token to delete is the last token, the swap operation is unnecessary
        if (tokenIndex != lastTokenIndex) {
            uint256 lastTokenId = _ownedListings[from][lastTokenIndex];

            _ownedListings[from][tokenIndex] = lastTokenId; // Move the last token to the slot of the to-delete token
           _ownedListingsIndex[lastTokenId] = tokenIndex; // Update the moved token's index
        }

        // This also deletes the contents at the last position of the array
        delete _ownedListingsIndex[tokenId];
        delete _ownedListings[from][lastTokenIndex];
        
        // Adjust the users total count
        _ownersTotalListings[from] = lastTokenIndex;
    }
}