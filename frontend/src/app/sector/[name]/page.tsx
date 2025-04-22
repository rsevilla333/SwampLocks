"use client";

import SectorDashboard from "../../components/SectorDashBoard";
import { useRouter } from "next/router";
import {useParams} from "next/navigation";

export default function SectorPage() {
    const { name } = useParams();

    if (!name || typeof name !== "string") return null;

    return (
        <SectorDashboard
            sectorName={name}
        />
    );
}
