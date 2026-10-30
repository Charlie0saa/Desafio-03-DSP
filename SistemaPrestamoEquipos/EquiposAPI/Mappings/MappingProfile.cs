using AutoMapper;
using EquiposAPI.DTOs;
using EquiposAPI.Models;

namespace EquiposAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos para Equipo con DTOs separados
            CreateMap<Equipo, EquipoDto>()
                .ForMember(dest => dest.NombreCategoria, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : string.Empty));

            CreateMap<CrearEquipoDto, Equipo>();
            CreateMap<ActualizarEquipoDto, Equipo>();

            // Mapeo faltante para registrar préstamos
            CreateMap<CrearPrestamoDto, Prestamo>();

            // Mapeos para los demás recursos
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Prestamo, PrestamoDto>().ReverseMap();
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
        }
    }
}