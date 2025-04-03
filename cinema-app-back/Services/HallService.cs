using System;
using System.Threading.Tasks;
using cinema_app_back.Models;
using cinema_app_back.DTOs;
using cinema_app_back.Repositories;
using AutoMapper;

namespace cinema_app_back.Services
{
    public class HallService
    {
        private readonly IHallRepository _hallRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<HallService> _logger;

        public HallService(
            IHallRepository hallRepository,
            IMapper mapper,
            ILogger<HallService> logger)
        {
            _hallRepository = hallRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<HallDto> CreateHallAsync(CreateHallDto createHallDto)
        {
            try
            {
                if (createHallDto.Rows <= 0 || createHallDto.SeatsPerRow <= 0)
                {
                    throw new ArgumentException("Кількість рядів та місць повинна бути більше 0");
                }

                var hall = new Hall
                {
                    Name = createHallDto.Name,
                    Rows = createHallDto.Rows,
                    SeatsPerRow = createHallDto.SeatsPerRow,
                    CinemaId = createHallDto.CinemaId
                };

                for (int row = 1; row <= createHallDto.Rows; row++)
                {
                    for (int seatNum = 1; seatNum <= createHallDto.SeatsPerRow; seatNum++)
                    {
                        var seat = new Seat
                        {
                            RowNumber = row,
                            SeatNumber = seatNum,
                            DisplayNumber = $"{(char)(64 + row)}{seatNum}",
                            IsAvailable = true,
                            IsReserved = false
                        };
                        hall.Seats.Add(seat);
                    }
                }

                // Зберігаємо зал разом з місцями
                await _hallRepository.AddAsync(hall);

                // Повертаємо створений зал
                return _mapper.Map<HallDto>(hall);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при створенні залу");
                throw;
            }
        }

        public async Task<HallDto> GetHallWithSeatsAsync(int id)
        {
            var hall = await _hallRepository.GetByIdWithSeatsAsync(id);
            return _mapper.Map<HallDto>(hall);
        }

        public async Task<IEnumerable<HallDto>> GetAllHallsAsync()
        {
            var halls = await _hallRepository.GetAllWithSeatsAsync();
            return _mapper.Map<IEnumerable<HallDto>>(halls);
        }
    }
} 