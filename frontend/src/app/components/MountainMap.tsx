"use client";
import React from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import { OrbitControls, Text, Billboard, Stars } from "@react-three/drei";
import { useMemo, useRef } from "react";
import * as THREE from "three";
import { Html } from "@react-three/drei";

//npm install three @react-three/fiber @react-three/drei

// Define the Stock interface.
interface Stock {
  symbol: string;
  marketCap: number;
  change: number;
}

interface ExtendedStock extends Stock {
  normalizedCap: number;
  baseRadius: number;
}

// Compute concentric positions (bullseye layout).
function computeConcentricPositions(
  stocks: Stock[],
  baseMin: number,
  baseMax: number
): { stock: ExtendedStock; position: { x: number; z: number } }[] {
  if (stocks.length === 0) return [];
  // Sort stocks descending by market cap.
  const sorted = stocks.slice().sort((a, b) => b.marketCap - a.marketCap);
  const marketCaps = sorted.map((s) => s.marketCap);
  const minCap = Math.min(...marketCaps);
  const maxCap = Math.max(...marketCaps);
  // Compute normalized market cap and base radius.
  
  const stocksWithData: ExtendedStock[] = sorted.map((stock) => {
    const normalizedCap = maxCap !== minCap ? (stock.marketCap - minCap) / (maxCap - minCap) : 0;
    const baseRadius = baseMin + normalizedCap * (baseMax - baseMin);
    return { ...stock, normalizedCap, baseRadius };
  });

  // Partition into rings: ring 0 has 1 item, ring r (r>=1) gets 6*r items.
  const rings: ExtendedStock[][] = [];
  let index = 0, ringNum = 0;
  while (index < stocksWithData.length) {
    const count = ringNum === 0 ? 1 : 6 * ringNum;
    rings.push(stocksWithData.slice(index, index + count));
    index += count;
    ringNum++;
  }

  // Compute the maximum base radius for each ring.
  const ringMax = rings.map((ring) => Math.max(...ring.map((s) => s.baseRadius), 0));

  // Compute ring radii.
  const ringR: number[] = [0];
  for (let i = 1; i < rings.length; i++) {
    const n = rings[i].length;
    const candidate1 = ringR[i - 1] + ringMax[i - 1] + ringMax[i];
    const candidate2 = n > 1 ? ringMax[i] / Math.sin(Math.PI / n) : candidate1;
    ringR[i] = Math.max(candidate1, candidate2);
  }

  // Assign positions.
  const positions: { stock: ExtendedStock; position: { x: number; z: number } }[] = [];
  if (rings.length > 0 && rings[0].length > 0) {
    positions.push({ stock: rings[0][0], position: { x: 0, z: 0 } });
  }
  for (let i = 1; i < rings.length; i++) {
    const n = rings[i].length;
    const r = ringR[i];
    for (let j = 0; j < n; j++) {
      const theta = (2 * Math.PI * j) / n;
      positions.push({ stock: rings[i][j], position: { x: r * Math.cos(theta), z: r * Math.sin(theta) } });
    }
  }
  return positions;
}

interface MountainProps {
  stock: ExtendedStock;
  position: { x: number; z: number };
}

function Mountain({ stock, position }: MountainProps) {
  const scaleFactor = 10;
  const height = Math.abs(stock.change) * scaleFactor;
  const isPositive = stock.change >= 0;
  const color = isPositive ? "#13d62a" : "#ed2424";

  const geometry = useMemo(() => {
    const geo = new THREE.ConeGeometry(stock.baseRadius, height, 4);
    geo.translate(0, height / 2, 0);
    return geo;
  }, [stock.baseRadius, height]);

  return (
      <>
        <mesh position={[position.x, 0, position.z]} rotation={isPositive ? [0, 0, 0] : [Math.PI, 0, 0]} geometry={geometry}>
          <meshStandardMaterial color={color} metalness={0.5} roughness={0.4} emissive={isPositive ? "#000000" : "#ff5555"} emissiveIntensity={0.4} />
        </mesh>
        <Html position={[position.x, isPositive ? height + 5 : -height - 5, position.z]}>
          <div style={{ color: "black", fontSize: "12px", textAlign: "center" }}>
            {stock.symbol} {stock.change}%
          </div>
        </Html>
      </>
  );
}

interface MountainChartProps {
  stocks: Stock[];
}

function MountainChart({ stocks }: MountainChartProps) {
  const baseMin = 3;
  const baseMax = 10;
  const positionsData = computeConcentricPositions(stocks, baseMin, baseMax);

  return (
      <>
        {positionsData.map((data, index) => (
            <Mountain key={data.stock.symbol + index} stock={data.stock} position={data.position} />
        ))}
      </>
  );
}


function RotatingGroup({ children }: { children: React.ReactNode }) {
  const groupRef = useRef<THREE.Group>(null!);
  useFrame((state, delta) => {
    groupRef.current.rotation.y += delta * 0.1;
  });
  return <group ref={groupRef}>{children}</group>;
}

interface MountainMapProps {
  stocks: Stock[];
}

const MountainMap: React.FC<MountainMapProps> = ({ stocks }) => {
  const containerWidth = 900;
  const containerHeight = 600;
  return (
    <div
      style={{
        width: `${containerWidth}px`,
        height: `${containerHeight}px`,
        borderRadius: "60% / 60%", // Oval container with horizontal long axis.
        // overflow: "hidden",
        backgroundColor: "transparent",
      }}
    >
      <Canvas
        shadows={false}
        camera={{ position: [0, 50, 100], fov: 65 }}
        style={{ background: "transparent" }}
      >
        <ambientLight intensity={0.3} />
        <directionalLight position={[0, 100, 50]} intensity={1} />
        <Stars radius={450} depth={50} count={5000} factor={4} saturation={0} fade />
        <RotatingGroup>
          <MountainChart stocks={stocks} />
        </RotatingGroup>
        <OrbitControls />
      </Canvas>
    </div>
  );
};

export default MountainMap;
