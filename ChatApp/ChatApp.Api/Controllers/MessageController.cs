using Microsoft.AspNetCore.Mvc;
using ChatApp.Domain.Services;
using ChatApp.Domain.Entities;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            var sentMessage = await _messageService.SendMessageAsync(message.ChatRoomId, message.UserId, message.Content);
            return CreatedAtAction(nameof(GetMessage), new { id = sentMessage.Id }, sentMessage);
        }

        [HttpGet("{id}")]
        public IActionResult GetMessage(int id)
        {
            var message = _messageService.GetMessage(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpGet]
        public IActionResult GetAllMessages()
        {
            var messages = _messageService.GetAllMessages();
            return Ok(messages);
        }
    }
}
