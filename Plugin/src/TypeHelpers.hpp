
// Opengl includes
#define GL_GLEXT_PROTOTYPES
#ifdef __APPLE__
#include <OpenGL/gl.h>
#include <OpenGL/glu.h>
#else
#ifdef _WIN32
  #include <windows.h>
#endif
#include <GL/gl.h>
#endif

/**
 * @brief Get the size of a pixel in bytes from the given internal format
 * 
 * @param internalFormat 
 * @return int The size of the pixel in number of bytes. 0 if not found
 */
inline int getPixelSizeFromInternalFormat(int internalFormat)
{
    switch(internalFormat) {
		case GL_R8: return 8;
		case GL_R8_SNORM: return 8;
		case GL_R16: return 16;
		case GL_R16_SNORM: return 16;
		case GL_RG8: return 8 + 8;
		case GL_RG8_SNORM: return 8 + 8;
		case GL_RG16: return 16 + 16;
		case GL_RG16_SNORM: return 16 + 16;
		case GL_R3_G3_B2: return 3 + 3 + 2;
		case GL_RGB4: return 4 + 4 + 4;
		case GL_RGB5: return 5 + 5 + 5;
		case GL_RGB8: return 8 + 8 + 8;
		case GL_RGB8_SNORM: return 8 + 8 + 8;
		case GL_RGB10: return 10 + 10 + 10;
		case GL_RGB12: return 12 + 12 + 12;
		case GL_RGB16: return 16 + 16 + 16;
		case GL_RGB16_SNORM: return 16 + 16 + 16;
		case GL_RGBA2: return 2 + 2 + 2 + 2;
		case GL_RGBA4: return 4 + 4 + 4 + 4;
		case GL_RGB5_A1: return 5 + 5 + 5 + 1;
		case GL_RGBA8: return 8 + 8 + 8 + 8;
		case GL_RGBA8_SNORM: return 8 + 8 + 8 + 8;
		case GL_RGB10_A2: return 10 + 10 + 10 + 2;
		case GL_RGBA12: return 12 + 12 + 12 + 12;
		case GL_RGBA16: return 16 + 16 + 16 + 16;
		case GL_RGBA16_SNORM: return 16 + 16 + 16 + 16;
		case GL_SRGB8: return 8 + 8 + 8;
		case GL_SRGB8_ALPHA8: return 8 + 8 + 8 + 8;
		case GL_R16F: return 16;
		case GL_RG16F: return 16 + 16;
		case GL_RGB16F: return 16 + 16 + 16;
		case GL_RGBA16F: return 16 + 16 + 16 + 16;
		case GL_R32F: return 32;
		case GL_RG32F: return 32 + 32;
		case GL_RGB32F: return 32 + 32 + 32;
		case GL_RGBA32F: return 32 + 32 + 32 + 32;
		case GL_R11F_G11F_B10F: return 11 + 11 + 10;
		case GL_RGB9_E5: return 9 + 9 + 9;
		case GL_R8I: return 8;
		case GL_R8UI: return 8;
		case GL_R16I: return 16;
		case GL_R16UI: return 16;
		case GL_R32I: return 32;
		case GL_R32UI: return 32;
		case GL_RG8I: return 8 + 8;
		case GL_RG8UI: return 8 + 8;
		case GL_RG16I: return 16 + 16;
		case GL_RG16UI: return 16 + 16;
		case GL_RG32I: return 32 + 32;
		case GL_RG32UI: return 32 + 32;
		case GL_RGB8I: return 8 + 8 + 8;
		case GL_RGB8UI: return 8 + 8 + 8;
		case GL_RGB16I: return 16 + 16 + 16;
		case GL_RGB16UI: return 16 + 16 + 16;
		case GL_RGB32I: return 32 + 32 + 32;
		case GL_RGB32UI: return 32 + 32 + 32;
		case GL_RGBA8I: return 8 + 8 + 8 + 8;
		case GL_RGBA8UI: return 8 + 8 + 8 + 8;
		case GL_RGBA16I: return 16 + 16 + 16 + 16;
		case GL_RGBA16UI: return 16 + 16 + 16 + 16;
		case GL_RGBA32I: return 32 + 32 + 32 + 32;
		case GL_RGBA32UI: return 32 + 32 + 32 + 32;
		case GL_DEPTH_COMPONENT16: return 16;
		case GL_DEPTH_COMPONENT24: return 24;
		case GL_DEPTH_COMPONENT32: return 32;
		case GL_DEPTH_COMPONENT32F: return 32;
		case GL_DEPTH24_STENCIL8: return 24 + 8;
		case GL_DEPTH32F_STENCIL8: return 32 + 8;
	}
	return 0;
}

/**
 * @brief Get the format from internal format
 * 
 * @param internalFormat 
 * @return int The size of the pixel in number of bytes.  0 if not found
 */
inline int getFormatFromInternalFormat(int internalFormat)
{
    switch(internalFormat) {
		case GL_R8:
		case GL_R8_SNORM:
		case GL_R16:
		case GL_R16_SNORM:
		case GL_R16F:
		case GL_R32F:
			return GL_RED;

		case GL_R8UI:
		case GL_R8I:
		case GL_R16UI:
		case GL_R16I:
		case GL_R32UI:
		case GL_R32I:
			return GL_RED_INTEGER;
		
		case GL_RG8:
		case GL_RG8_SNORM:
		case GL_RG16:
		case GL_RG16_SNORM:
		case GL_RG16F:
		case GL_RG32F:
			return GL_RG;

		case GL_RG8UI:
		case GL_RG8I:
		case GL_RG16UI:
		case GL_RG16I:
		case GL_RG32UI:
		case GL_RG32I:
			return GL_RG_INTEGER;

		case GL_RGB8:
		case GL_RGB8_SNORM:
		case GL_RGB16:
		case GL_RGB16_SNORM:
		case GL_RGB16F:
		case GL_RGB32F:
			return GL_RGB;

		case GL_RGB8UI:
		case GL_RGB8I:
		case GL_RGB16UI:
		case GL_RGB16I:
		case GL_RGB32UI:
		case GL_RGB32I:
			return GL_RGB_INTEGER;

		case GL_RGBA8:
		case GL_RGBA8_SNORM:
		case GL_RGBA16:
		case GL_RGBA16_SNORM:
		case GL_RGBA16F:
		case GL_RGBA32F:
			return GL_RGBA;

		case GL_RGBA8UI:
		case GL_RGBA8I:
		case GL_RGBA16UI:
		case GL_RGBA16I:
		case GL_RGBA32UI:
		case GL_RGBA32I:
			return GL_RGBA_INTEGER;

		case GL_SRGB8:
		case GL_SRGB8_ALPHA8:
			return GL_RGB;
	}
	return 0;
}

/**
 * @brief Get the best suitable type from internal format
 * 
 * @param internalFormat 
 * @return int The size of the pixel in number of bytes.  0 if not found
 */
inline int getTypeFromInternalFormat(int internalFormat)
{
    switch(internalFormat) {
		case GL_R8:
		case GL_RG8:
		case GL_RGB8:
		case GL_RGBA8:
		case GL_SRGB8:
		case GL_SRGB8_ALPHA8:
		case GL_R8UI:
		case GL_RG8UI:
		case GL_RGB8UI:
		case GL_RGBA8UI:
			return GL_UNSIGNED_BYTE;

		case GL_R8_SNORM:
		case GL_RG8_SNORM:
		case GL_RGB8_SNORM:
		case GL_RGBA8_SNORM:
		case GL_R8I:
		case GL_RG8I:
		case GL_RGB8I:
		case GL_RGBA8I:
			return GL_BYTE;

		case GL_R16:
		case GL_RG16:
		case GL_RGB16:
		case GL_RGBA16:
		case GL_R16UI:
		case GL_RG16UI:
		case GL_RGB16UI:
		case GL_RGBA16UI:
			return GL_UNSIGNED_SHORT;

		case GL_R16_SNORM:
		case GL_RG16_SNORM:
		case GL_RGB16_SNORM:
		case GL_RGBA16_SNORM:
		case GL_R16I:
		case GL_RG16I:
		case GL_RGB16I:
		case GL_RGBA16I:
			return GL_SHORT;

		case GL_R16F:
		case GL_RG16F:
		case GL_RGB16F:
		case GL_RGBA16F:
			return GL_HALF_FLOAT;

		case GL_R32F:
		case GL_RG32F:
		case GL_RGB32F:
		case GL_RGBA32F:
			return GL_FLOAT;

		case GL_R32UI:
		case GL_RG32UI:
		case GL_RGB32UI:
		case GL_RGBA32UI:
			return GL_UNSIGNED_INT;

		case GL_R32I:
		case GL_RG32I:
		case GL_RGB32I:
		case GL_RGBA32I:
			return GL_INT;
	}
	return 0;
}