using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoPlaces.Events;
using GoPlaces.ExternalApiMetrics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GoPlaces.Tests.Events
{
    public class EventAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly EventAppService _eventAppService;

        // Mocks
        private readonly Mock<ITicketMasterEventSearchService> _mockTicketMasterEventSearchService;
        private readonly Mock<IRepository<Event, Guid>> _mockEventRepository;
        private readonly Mock<IRepository<ExternalApiCall, Guid>> _mockExternalApiCallRepository;

        public EventAppService_Tests()
        {
            _mockTicketMasterEventSearchService = new Mock<ITicketMasterEventSearchService>();
            _mockEventRepository = new Mock<IRepository<Event, Guid>>();
            _mockExternalApiCallRepository = new Mock<IRepository<ExternalApiCall, Guid>>();

            // Por defecto no hay eventos previos guardados (el upsert por TicketMasterId no encuentra nada)
            _mockEventRepository
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event)null!);

            _mockEventRepository
                .Setup(r => r.InsertAsync(It.IsAny<Event>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event e, bool _, CancellationToken __) => e);

            _mockExternalApiCallRepository
                .Setup(r => r.InsertAsync(It.IsAny<ExternalApiCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ExternalApiCall e, bool _, CancellationToken __) => e);

            _eventAppService = new EventAppService(
                _mockTicketMasterEventSearchService.Object,
                _mockEventRepository.Object,
                _mockExternalApiCallRepository.Object,
                new EventSearchDomainService()
            );

            // Necesario para que Logger/GuidGenerator/AsyncExecuter no exploten al resolverse
            // fuera del contenedor de DI (mismo approach que RatingAppService_Tests.CreateRatingService()).
            _eventAppService.LazyServiceProvider = ServiceProvider.GetRequiredService<IAbpLazyServiceProvider>();
        }

        [Fact]
        public async Task SearchEventsByCityAsync_devuelve_resultados_del_servicio()
        {
            // ARRANGE
            var request = new EventSearchRequestDto { City = "Buenos Aires" };

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.Is<EventSearchRequestDto>(r => r.City == "Buenos Aires")))
                .ReturnsAsync(new EventSearchResultDto
                {
                    Events = new List<EventDto>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Concierto de Rock",
                            StartDate = new DateTime(2026, 8, 1),
                            Venue = "Movistar Arena",
                            City = "Buenos Aires",
                            Url = "https://ticketmaster.com/evento/1",
                            TicketMasterId = "TM-1"
                        }
                    }
                });

            // ACT
            var result = await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT
            result.ShouldNotBeNull();
            result.Events.Count.ShouldBe(1);
            result.Events[0].Name.ShouldBe("Concierto de Rock");
            result.Events[0].Venue.ShouldBe("Movistar Arena");
            result.Events[0].TicketMasterId.ShouldBe("TM-1");
        }

        [Fact]
        public async Task SearchEventsByCityAsync_devuelve_resultado_vacio_cuando_no_hay_eventos()
        {
            // ARRANGE
            var request = new EventSearchRequestDto { City = "Ciudad_Sin_Eventos" };

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.Is<EventSearchRequestDto>(r => r.City == "Ciudad_Sin_Eventos")))
                .ReturnsAsync(new EventSearchResultDto { Events = new List<EventDto>() });

            // ACT
            var result = await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT
            result.ShouldNotBeNull();
            result.Events.ShouldBeEmpty();
        }

        [Fact]
        public async Task SearchEventsByCityAsync_lanza_excepcion_si_no_se_especifica_ciudad()
        {
            // ARRANGE
            var request = new EventSearchRequestDto { City = "" };

            // ACT & ASSERT
            await Should.ThrowAsync<UserFriendlyException>(
                () => _eventAppService.SearchEventsByCityAsync(request));

            // La validación de dominio debe cortar antes de llamar a la API externa
            _mockTicketMasterEventSearchService.Verify(
                s => s.SearchEventsAsync(It.IsAny<EventSearchRequestDto>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchEventsByCityAsync_lanza_excepcion_si_rango_de_fechas_es_invalido()
        {
            // ARRANGE
            var request = new EventSearchRequestDto
            {
                City = "Cordoba",
                StartDateFrom = new DateTime(2026, 12, 31),
                StartDateTo = new DateTime(2026, 1, 1)
            };

            // ACT & ASSERT
            await Should.ThrowAsync<UserFriendlyException>(
                () => _eventAppService.SearchEventsByCityAsync(request));
        }

        [Fact]
        public async Task SearchEventsByCityAsync_captura_error_de_api_externa_y_no_relanza()
        {
            // ARRANGE
            var request = new EventSearchRequestDto { City = "Madrid" };

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.IsAny<EventSearchRequestDto>()))
                .ThrowsAsync(new HttpRequestException("Error simulado de conexión con TicketMaster"));

            // ACT
            var result = await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT: no debe relanzar, debe devolver un resultado vacío
            result.ShouldNotBeNull();
            result.Events.ShouldBeEmpty();
        }

        [Fact]
        public async Task SearchEventsByCityAsync_trackea_ExternalApiCall_exitoso()
        {
            // ARRANGE
            var request = new EventSearchRequestDto { City = "Buenos Aires" };

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.IsAny<EventSearchRequestDto>()))
                .ReturnsAsync(new EventSearchResultDto { Events = new List<EventDto>() });

            // ACT
            await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT
            _mockExternalApiCallRepository.Verify(
                r => r.InsertAsync(
                    It.Is<ExternalApiCall>(c => c.ApiName == "TicketMaster" && c.IsSuccess),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchEventsByCityAsync_trackea_ExternalApiCall_fallido_cuando_la_api_explota()
        {
            // ARRANGE
            var request = new EventSearchRequestDto { City = "Madrid" };

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.IsAny<EventSearchRequestDto>()))
                .ThrowsAsync(new HttpRequestException("Error simulado de conexión con TicketMaster"));

            // ACT
            await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT
            _mockExternalApiCallRepository.Verify(
                r => r.InsertAsync(
                    It.Is<ExternalApiCall>(c => c.ApiName == "TicketMaster" && !c.IsSuccess),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchEventsByCityAsync_persiste_el_DestinationId_correctamente()
        {
            // ARRANGE
            var destinationId = Guid.NewGuid();
            var request = new EventSearchRequestDto { City = "Buenos Aires", DestinationId = destinationId };

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.IsAny<EventSearchRequestDto>()))
                .ReturnsAsync(new EventSearchResultDto
                {
                    Events = new List<EventDto>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Concierto de Rock",
                            StartDate = new DateTime(2026, 8, 1),
                            Venue = "Movistar Arena",
                            City = "Buenos Aires",
                            TicketMasterId = "TM-100"
                        }
                    }
                });

            // ACT
            await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT: el evento nuevo se inserta ya vinculado al destino esperado
            _mockEventRepository.Verify(
                r => r.InsertAsync(
                    It.Is<Event>(e => e.TicketMasterId == "TM-100" && e.DestinationId == destinationId),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchEventsByCityAsync_vincula_el_DestinationId_a_un_evento_que_ya_existia_sin_destino()
        {
            // Reproduce el bug reportado: el evento de TicketMaster ya había sido sincronizado antes
            // (ej. una búsqueda anterior sin destino asociado), por lo que ya existe en la tabla con
            // DestinationId = null. Al volver a sincronizar con un destino, debe quedar vinculado en
            // vez de ser ignorado silenciosamente por el chequeo de duplicados (mismo TicketMasterId).

            // ARRANGE
            var destinationId = Guid.NewGuid();
            var ticketMasterId = "TM-42";

            var eventoExistente = new Event(
                Guid.NewGuid(),
                "Concierto Existente",
                new DateTime(2026, 9, 1),
                "Luna Park",
                "Buenos Aires",
                ticketMasterId
            );
            // Nace sin destino asociado, tal como quedaría tras una sincronización previa sin destinationId
            eventoExistente.DestinationId.ShouldBeNull();

            _mockEventRepository
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventoExistente);

            _mockEventRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Event>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event e, bool _, CancellationToken __) => e);

            _mockTicketMasterEventSearchService
                .Setup(s => s.SearchEventsAsync(It.IsAny<EventSearchRequestDto>()))
                .ReturnsAsync(new EventSearchResultDto
                {
                    Events = new List<EventDto>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Concierto Existente",
                            StartDate = new DateTime(2026, 9, 1),
                            Venue = "Luna Park",
                            City = "Buenos Aires",
                            TicketMasterId = ticketMasterId
                        }
                    }
                });

            var request = new EventSearchRequestDto { City = "Buenos Aires", DestinationId = destinationId };

            // ACT
            await _eventAppService.SearchEventsByCityAsync(request);

            // ASSERT: el evento existente debe haber quedado vinculado al destino
            eventoExistente.DestinationId.ShouldBe(destinationId);

            _mockEventRepository.Verify(
                r => r.UpdateAsync(
                    It.Is<Event>(e => e.TicketMasterId == ticketMasterId && e.DestinationId == destinationId),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // No debe crearse un duplicado del mismo evento
            _mockEventRepository.Verify(
                r => r.InsertAsync(It.IsAny<Event>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
