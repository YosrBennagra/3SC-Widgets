# 🎯 Logo Size Tester Widget

A powerful design tool widget for testing logo visibility and legibility at different sizes - from tiny favicons to massive billboards.

## ✨ Features

### Size Previews
- **Favicon** (16×16px) - Browser tabs, bookmarks
- **Mobile Icon** (64×64px) - App icons, touch icons
- **Desktop** (256×256px) - Websites, social media
- **Billboard** (512×512px) - Large displays, high-resolution screens

### Background Testing
Test your logo against 8 different backgrounds:
- White, Light Gray, Gray
- Dark Gray, Black
- Blue, Green, Red

### Contrast Analysis
- **WCAG Contrast Ratio** - Automatic calculation
- **Accessibility Rating** - AAA, AA, Fair, or Poor
- **Real-time Updates** - Changes as you switch backgrounds

### Legibility Detection
- **Minimum Legible Size** - Heuristic analysis
- **Detail Detection** - Identifies logos with fine details
- **Aspect Ratio Analysis** - Accounts for wide/tall logos

### Export Functionality
- **Multi-size Export** - Exports all 4 sizes at once
- **PNG Format** - High-quality transparent backgrounds
- **Named Files** - `logo_favicon.png`, `logo_mobile.png`, etc.

## 🎨 Use Cases

### For Designers
- Test logo designs before finalization
- Ensure legibility at small sizes
- Verify contrast on different backgrounds
- Export multiple sizes for developers

### For Developers
- Validate favicons and app icons
- Test responsive logo implementations
- Generate required icon sizes quickly
- Check accessibility compliance

### For Brand Managers
- Ensure brand consistency across sizes
- Test logos on competitor backgrounds
- Validate logo visibility standards
- Document legibility requirements

## 🚀 Usage

### 1. Upload Logo
Click the upload button (📤) or "Choose File" to select your logo image.

Supported formats:
- PNG (recommended for logos with transparency)
- JPG/JPEG
- BMP
- GIF

### 2. Test Backgrounds
Click the background buttons to see how your logo looks:
- Test on brand colors (Blue, Green, Red)
- Test on neutral backgrounds (White, Gray, Black)
- Verify contrast ratios meet WCAG guidelines

### 3. Analyze Results
Review the analysis panel:
- **Contrast Ratio**: Should be ≥4.5:1 for AA, ≥7.0:1 for AAA
- **Min Legible Size**: Recommended minimum size for clarity
- Visual feedback with emoji indicators (✅✓⚠❌)

### 4. Export Sizes
Click "💾 Export All Sizes" to save all four size variants to a folder.

## 📐 Size Reference Guide

| Size | Dimensions | Common Uses |
|------|-----------|-------------|
| **Favicon** | 16×16px | Browser tabs, bookmarks, address bar |
| **Mobile** | 64×64px | iOS/Android app icons, PWA icons |
| **Desktop** | 256×256px | Website headers, social media, email |
| **Billboard** | 512×512px | Hero images, print materials, HD displays |

## ⚡ Technical Details

### Contrast Calculation
Uses WCAG 2.1 relative luminance formula:
- Converts colors to linear RGB
- Calculates relative luminance
- Computes contrast ratio: (L1 + 0.05) / (L2 + 0.05)

### Legibility Heuristics
- Aspect ratio analysis (wide/tall logos need more space)
- Resolution detection (high-res images may have fine details)
- Minimum size recommendations based on complexity

### Image Processing
- High-quality bitmap scaling
- Transparent PNG export
- Preserves original aspect ratio
- Samples pixels for color analysis

## 🎯 Design Tips

### For Small Sizes (Favicon, Mobile)
- ✅ Use simple, bold shapes
- ✅ Avoid fine details or thin lines
- ✅ High contrast between elements
- ✅ Test on multiple backgrounds

### For Medium Sizes (Desktop)
- ✅ Balance detail and simplicity
- ✅ Ensure text is readable
- ✅ Test on brand colors
- ✅ Verify 4.5:1+ contrast ratio

### For Large Sizes (Billboard)
- ✅ Details become visible
- ✅ Test scalability
- ✅ Check for pixelation
- ✅ Verify vector quality

## 🏗️ Widget Info

- **Widget Key**: `logo-size-tester`
- **Version**: 1.0.0
- **Size**: 500×660px (default)
- **Framework**: WPF .NET 8.0
- **MVVM**: CommunityToolkit.Mvvm 8.2.2

## 📝 Future Enhancements

Potential features for future versions:
- [ ] Custom background color picker
- [ ] Pattern backgrounds (checkerboard, gradient)
- [ ] Side-by-side comparison mode
- [ ] Logo history/favorites
- [ ] SVG logo support
- [ ] Auto-detect optimal minimum size
- [ ] Batch processing multiple logos
- [ ] Export preset packs (web, mobile, print)

## 🐛 Troubleshooting

**Logo not loading:**
- Ensure file is a supported image format
- Check file permissions
- Try a different image

**Contrast ratio showing N/A:**
- Upload a logo first
- Try a different background color
- Ensure logo has non-transparent pixels

**Export not working:**
- Check folder permissions
- Ensure enough disk space
- Try a different export location

## 📄 License

Part of the 3SC Widgets Collection
External Community Widget for 3SC Desktop Application

---

**Created with** ❤️ **for designers, developers, and brand managers**
