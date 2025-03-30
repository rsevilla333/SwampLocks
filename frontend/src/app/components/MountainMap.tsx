"use client";
import React from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import { OrbitControls, Text, Billboard, Stars } from "@react-three/drei";
import { useMemo, useRef } from "react";
import * as THREE from "three";

//npm install three @react-three/fiber @react-three/drei

// Define the Stock interface.
interface Stock {
  symbol: string;
  marketCap: number;
  change: number;
}

// Compute concentric positions (bullseye layout).
function computeConcentricPositions(
  stocks: Stock[],
  baseMin: number,
  baseMax: number
) {
  // Sort stocks descending by market cap.
  const sorted = stocks.slice().sort((a, b) => b.marketCap - a.marketCap);
  const marketCaps = sorted.map((s) => s.marketCap);
  const minCap = Math.min(...marketCaps);
  const maxCap = Math.max(...marketCaps);
  // Compute normalized market cap and base radius.
  const stocksWithData = sorted.map((stock) => {
    const normalizedCap = (stock.marketCap - minCap) / (maxCap - minCap);
    const baseRadius = baseMin + normalizedCap * (baseMax - baseMin);
    return { ...stock, normalizedCap, baseRadius };
  });

  // Partition into rings: ring 0 has 1 item, ring r (r>=1) gets 6*r items.
  const rings: Stock[][] = [];
  let index = 0;
  let ringNum = 0;
  while (index < stocksWithData.length) {
    if (ringNum === 0) {
      rings.push(stocksWithData.slice(index, index + 1));
      index += 1;
    } else {
      const count = 6 * ringNum;
      rings.push(stocksWithData.slice(index, index + count));
      index += count;
    }
    ringNum++;
  }

  // Compute the maximum base radius for each ring.
  const ringMax = rings.map((ring) => Math.max(...ring.map((s) => s.baseRadius)));
  // Compute ring radii.
  const ringR: number[] = [0];
  for (let i = 1; i < rings.length; i++) {
    const n = rings[i].length;
    const candidate1 = ringR[i - 1] + ringMax[i - 1] + ringMax[i];
    const candidate2 = n > 1 ? ringMax[i] / Math.sin(Math.PI / n) : candidate1;
    ringR[i] = Math.max(candidate1, candidate2);
  }

  // Assign positions.
  const positions: { stock: Stock; position: { x: number; z: number } }[] = [];
  if (rings.length > 0 && rings[0].length > 0) {
    positions.push({ stock: rings[0][0], position: { x: 0, z: 0 } });
  }
  for (let i = 1; i < rings.length; i++) {
    const n = rings[i].length;
    const r = ringR[i];
    for (let j = 0; j < n; j++) {
      const theta = (2 * Math.PI * j) / n;
      const x = r * Math.cos(theta);
      const z = r * Math.sin(theta);
      positions.push({ stock: rings[i][j], position: { x, z } });
    }
  }
  return positions;
}

interface MountainProps {
  stock: Stock & { normalizedCap: number; baseRadius: number };
  position: { x: number; z: number };
}

function Mountain({ stock, position, normalizedCap }: MountainProps) {
  const scaleFactor = 10; // Height scaling factor for percent change.
  const height = Math.abs(stock.change) * scaleFactor;
  const isPositive = stock.change >= 0;
  const color = isPositive ? "#13d62a" : "#ed2424";
  const baseMin = 3;
  const baseMax = 10;
  const baseRadius = baseMin + normalizedCap * (baseMax - baseMin);

  const geometry = useMemo(() => {
    const geo = new THREE.ConeGeometry(baseRadius, height, 4);
    // Translate so that the cone's base sits at y = 0.
    geo.translate(0, height / 2, 0);
    return geo;
  }, [baseRadius, height]);

  return (
    <>
      <mesh
        position={[position.x, 0, position.z]}
        rotation={isPositive ? [0, 0, 0] : [Math.PI, 0, 0]}
        geometry={geometry}
      >
        <meshStandardMaterial
          attach="material"
          color={color}
          metalness={0.5}
          roughness={0.4}
          emissive={isPositive ? "#000000" : "#ff5555"}
          emissiveIntensity={0.4}
        />
      </mesh>
      <Billboard position={[position.x, isPositive ? height + 3 : -height - 3, position.z]}>
        {/* ISSUE HERE V SUPPOSED TO DISPLAY TICKER AND PERCENTAGE */}
        {/* <Text
            fontSize={3} color="white" anchorX="center" anchorY="middle"
            position={[0, isPositive ? height + 3 : -height - 3, 0]}
            >
            {stock.symbol} {stock.change}%
        </Text> */}
      </Billboard>
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
        <Mountain
          key={data.stock.symbol + index}
          stock={data.stock as Stock & { normalizedCap: number; baseRadius: number }}
          position={data.position}
          normalizedCap={data.stock.normalizedCap}
        />
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
        borderRadius: "50% / 50%", // Oval container with horizontal long axis.
        overflow: "hidden",
        backgroundColor: "#000",
      }}
    >
      <Canvas
        shadows={false}
        camera={{ position: [0, 50, 150], fov: 69 }}
        style={{ background: "#000" }}
      >
        <ambientLight intensity={0.3} />
        <directionalLight position={[0, 100, 50]} intensity={1} />
        <Stars radius={300} depth={50} count={5000} factor={4} saturation={0} fade />
        <RotatingGroup>
          <MountainChart stocks={stocks} />
        </RotatingGroup>
        <OrbitControls />
      </Canvas>
    </div>
  );
};

export default MountainMap;
