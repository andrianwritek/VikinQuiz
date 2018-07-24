﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VikingQuiz.Api.Models;
using VikingQuiz.Api.Repositories;
using VikingQuiz.Api.Mappers;
using VikingQuiz.Api.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VikingQuiz.Api.Controllers
{
    [Route("api/[controller]")]
    public class PlayerController : Controller
    {
        private readonly PlayerRepo playerRepo;
        private IEntityMapper<Player, PlayerViewModel> entityToVmMapper;
        private IEntityMapper<PlayerViewModel, Player> vmToEntityMapper;

        public PlayerController(PlayerRepo playerRepo, IEntityMapper<Player, PlayerViewModel> entityToVmMapper, IEntityMapper<PlayerViewModel, Player> vmToEntityMapper)
        {
            this.playerRepo = playerRepo;
            this.entityToVmMapper = entityToVmMapper;
            this.vmToEntityMapper = vmToEntityMapper;
        }

        [HttpGet]
        public IActionResult GetPlayer()
        {
            var result = playerRepo.GetAllPlayers().Select(s => entityToVmMapper.Map(s)).ToList();
            return Ok(result);
        }

        [HttpPost]
        public IActionResult CreatePlayer([FromBody]PlayerViewModel player)
        {
            Player p = new Player()
            {
                PictureUrl = player.PictureUrl,
                Name = player.Name
            };

            Player newPlayer = playerRepo.AddPlayer(p);
            if(newPlayer == null)
            {
                return BadRequest("Player couldn't be created");
            }
            PlayerViewModel playerVm = entityToVmMapper.Map(p);
            return Ok(playerVm);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePlayer(int id, [FromBody]PlayerViewModel player)
        {
            Player p = new Player()
            {
                Id = id,
                PictureUrl = player.PictureUrl,
                Name = player.Name
            };

            Player updatedPlayer = playerRepo.UpdatePlayer(p);
            if (updatedPlayer == null)
            {
                return BadRequest("Player couldn't be updated");
            }
            PlayerViewModel playerVm = entityToVmMapper.Map(updatedPlayer);
            return Ok(playerVm);
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePlayer(int id)
        {
            playerRepo.DeletePlayer(id);
            return Ok();
        }

    }
}