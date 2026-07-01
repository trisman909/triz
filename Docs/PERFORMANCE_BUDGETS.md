# Lanternfall performance budgets

These are release gates, not aspirations. Measurements use a non-development
Windows x64 player at 1920×1080, Low preset, after a 60-second warm-up.

## Declared minimum Windows PC

- Windows 10 64-bit
- Intel Core i5-8400 or AMD Ryzen 5 2600
- NVIDIA GTX 1060 6 GB or AMD RX 580 8 GB
- 8 GB system memory and an SSD

## Frame and memory budgets

| Metric | Budget |
|---|---:|
| Frame time | 16.67 ms average, at most 5% over budget |
| Main thread | 8 ms average |
| GPU | 14 ms average |
| Steady-state managed allocation | 0 B/frame in combat hot paths |
| Total player memory | 2 GB |
| Concurrent standard enemies | 60 |
| Concurrent guardian | 1 |
| Prewarmed projectiles | 32 per player pool |

`FrameBudgetMonitor` uses a fixed 300-sample ring and performs no per-frame
managed allocation. The Phase 11 PlayMode gate exercises the first-biome
vertical slice for 180 frames and rejects frame or population budget breaches.
Final hardware capture is repeated for the Phase 12 release candidate.
