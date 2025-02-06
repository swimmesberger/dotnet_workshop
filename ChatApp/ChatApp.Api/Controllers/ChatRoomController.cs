using Microsoft.AspNetCore.Mvc;
using ChatApp.Domain.Services;
using ChatApp.Domain.Entities;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatRoomController : ControllerBase
    {
        private readonly ChatRoomService _chatRoomService;

        public ChatRoomController(ChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChatRoom([FromBody] string name)
        {
            var chatRoom = await _chatRoomService.CreateChatRoomAsync(name);
            return CreatedAtAction(nameof(GetChatRoom), new { id = chatRoom.Id }, chatRoom);
        }

        [HttpGet("{id}")]
        public IActionResult GetChatRoom(int id)
        {
            var chatRoom = _chatRoomService.GetChatRoom(id);
            if (chatRoom == null)
            {
                return NotFound();
            }
            return Ok(chatRoom);
        }

        [HttpGet]
        public IActionResult GetAllChatRooms()
        {
            var chatRooms = _chatRoomService.GetAllChatRooms();
            return Ok(chatRooms);
        }
    }
}
