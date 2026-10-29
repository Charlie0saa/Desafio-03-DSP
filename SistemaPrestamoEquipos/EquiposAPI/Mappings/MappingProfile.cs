
using AutoMapper;
using EquiposAPI.DTOs;
using EquiposAPI.Models;

namespace EquiposAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos para Categoría
            CreateMap<Categoria, CategoriaDto>().ReverseMap();

            // Mapeos para Equipo
            CreateMap<Equipo, EquipoDto>()
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : string.Empty));
            CreateMap<EquipoDto, Equipo>();

            // Mapeos para Usuario
            CreateMap<Usuario, UsuarioDto>();
            CreateMap<RegisterUserDto, Usuario>();

            // Mapeos para Préstamo
            CreateMap<Prestamo, PrestamoDto>()
                .ForMember(dest => dest.EquipoNombre, opt => opt.MapFrom(src => src.Equipo != null ? src.Equipo.Nombre : string.Empty))
                .ForMember(dest => dest.CodigoInventario, opt => opt.MapFrom(src => src.Equipo != null ? src.Equipo.CodigoInventario : string.Empty))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Username : string.Empty));

            CreateMap<CrearPrestamoDto, Prestamo>();
        }
    }
}
