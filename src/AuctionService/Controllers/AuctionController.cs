using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAuctionRepository _auctionRepository;

    public AuctionController(IMapper mapper, IPublishEndpoint publishEndpoint, IAuctionRepository auctionRepository)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _auctionRepository = auctionRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _auctionRepository.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);

        if (auction is null) return NotFound();

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);

        auction.Seller = User.Identity.Name;

        _auctionRepository.AddAuction(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _auctionRepository.SaveChangesAsync();

        if (!result) return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }


    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _auctionRepository.GetAuctionEntityById(id);

        if (auction is null) return NotFound();

        if (!string.Equals(auction.Seller, User.Identity.Name)) return Forbid();
        
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _auctionRepository.SaveChangesAsync();

        if (!result) BadRequest("Problem saving changes");

        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _auctionRepository.GetAuctionEntityById(id);

        if (auction is null) return NotFound();

        if (!string.Equals(auction.Seller, User.Identity.Name)) return Forbid();
        
        _auctionRepository.RemoveAuction(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _auctionRepository.SaveChangesAsync();

        if (!result) return BadRequest("Could not update BD");

        return Ok();
    }
}