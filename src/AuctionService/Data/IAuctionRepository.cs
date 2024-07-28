using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
    Task<AuctionDto> GetAuctionByIdAsync(Guid id);
    
    Task<List<AuctionDto>> GetAuctionsAsync(string date);

    Task<Auction> GetAuctionEntityById(Guid id);

    void AddAuction(Auction auction);

    void RemoveAuction(Auction auction);

    Task<bool> SaveChangesAsync();

}