"use client";

import { MainLayout } from "@/components/layouts/MainLayout";
import { UserProfilePage } from "@/components/pages/UserProfilePage";
import { useParams } from "next/navigation";

export default function UserProfile() {
  const params = useParams();
  const id = params.id as string;

  return (
    <MainLayout>
      <UserProfilePage userId={id} />
    </MainLayout>
  );
}
